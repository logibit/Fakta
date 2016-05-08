///The Agent endpoints are used to interact with the local Consul agent.
/// Usually, services and checks are registered with an agent which then takes
/// on the burden of keeping that data synchronized with the cluster. For
/// example, the agent registers services and checks with the Catalog and
/// performs anti-entropy to recover from outages.
module Fakta.Agent
open System
open System.Text
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open NodaTime
open HttpFs.Client
open Chiron
open Hopac

let agentPath (funcName: string) =
  [| "Fakta"; "Agent"; funcName |]

let writeFilters state =
  agentPath >> writeFilters state

let queryFilters state =
  agentPath >> queryFilters state

let checkDeregister state : WriteCall<Id, unit> =
  let createRequest =
    writeCallEntity state.config "agent/check/deregister"

  let filters =
    writeFilters state "checkDeregister"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

/// The register endpoint is used to add a new check to the local agent. There
/// is more documentation on checks here. Checks may be of script, HTTP, TCP, or
/// TTL type. The agent is responsible for managing the status of the check and
/// keeping the Catalog in sync.
///
/// This endpoint supports ACL tokens. If the query string includes a
/// ?token=<token-id>, the registration will use the provided token to authorize
/// the request. The token is also persisted in the agent's local configuration
/// to enable periodic anti-entropy syncs and seamless agent restarts.
let checkRegister state : WriteCall<AgentCheckRegistration, unit> =
  let createRequest (registration, opts) =
    writeCallUri state.config "agent/check/register" opts
    |> basicRequest state.config Put
    |> withJsonBodyT registration

  let filters =
    writeFilters state "checkRegister"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

/// This endpoint is used to return all the checks that are registered with the
/// local agent. These checks were either provided through configuration files
/// or added dynamically using the HTTP API. It is important to note that the
/// checks known by the agent may be different from those reported by the
/// Catalog. This is usually due to changes being made while there is no leader
/// elected. The agent performs active anti-entropy, so in most situations
/// everything will be in sync within a few seconds.
let checks state : QueryCall<Map<string, AgentCheck>> =
  let createRequest =
    queryCall state.config "agent/checks"

  let filters =
    queryFilters state "list"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

let private setNodeMaintenance enable state : WriteCall<string, unit> =
  let createRequest (reason, opts) =
    writeCall state.config "agent/maintenance" opts
    |> Request.queryStringItem "enable" (string enable |> String.toLowerInvariant)
    |> Request.queryStringItem "reason" reason

  let filters =
    writeFilters state "setNodeMaintenance"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let enableNodeMaintenance state =
  setNodeMaintenance true state

let disableNodeMaintenance state =
  setNodeMaintenance false state

let setServiceMaintenanceMode (state : FaktaState) (enable : bool) (serviceId : string) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "service/maintenance/%s" serviceId)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ yield "enable", Some((enable.ToString().ToLower())) ]
  let! result = call state (agentPath "service.maintenance") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// DisableServiceMaintenance toggles service maintenance mode off for the given service id.
let disableServiceMaintenance (state : FaktaState) (serviceId : Id) : Job<Choice<unit, Error>> =
  setServiceMaintenanceMode state false (serviceId.ToString())

/// EnableServiceMaintenance toggles service maintenance mode on for the given service id.
let enableServiceMaintenance (state : FaktaState) (serviceId : Id) (reason : string) : Job<Choice<unit, Error>> =
  setServiceMaintenanceMode state true (serviceId.ToString())

/// Join is used to instruct the agent to attempt a join to another cluster member
let join (state : FaktaState) (addr : string) (wan : bool) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "join/%s" addr)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ yield "wan", if wan then Some("1") else Some("0") ]
  let! result = call state (agentPath "join") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Members returns the known gossip members. The WAN flag can be used to query a server for WAN members.
let members (state : FaktaState) (wan : bool) : Job<Choice<AgentMember list, Error>> = job {
  let urlPath = (sprintf "members")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let  item = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Self is used to query the agent we are speaking to for information about itself
let self (state : FaktaState) : Job<Choice<Map<string, Map<string, Chiron.Json>>, Error>> = job {
  let urlPath = (sprintf "self")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let  item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// NodeName is used to get the node name of the agent
let nodeName (state : FaktaState) : Job<Choice<string, Error>> = job {
  let! result = self state
  match result with
  | Choice1Of2 map ->
      let r, err =
        match map.TryFind("Config") with
        | Some config ->
            match config.TryFind("NodeName") with
              | Some nodeNameJson ->
                  let nodeName = Json.format nodeNameJson
                  nodeName, false
              | None -> "Node name not found", true
        | None -> "Config section not found", true
      if err =  false
        then return Choice1Of2(r)
        else return Choice2Of2 (Message r)
  | Choice2Of2 err ->  return Choice2Of2 (err)
}

/// ServiceDeregister is used to deregister a service with the local agent
let serviceDeregister (state : FaktaState) (serviceId : Id) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "check/deregister/%s" serviceId)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentPath "service.deregister") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// ServiceRegister is used to register a new service with the local agent
let serviceRegister (state : FaktaState) (service : AgentServiceRegistration) : Job<Choice<unit, Error>> = job {
  let urlPath = "service/register"
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let serializedCheckReg = Json.serialize service
  let! result = call state (agentPath "service.register") (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Services returns the locally registered services
let services (state : FaktaState) : Job<Choice<Map<string, AgentService>, Error>> = job {
  let urlPath = "services"
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let  item:Map<string,AgentService> = if body = "" then Map.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// UpdateTTL is used to update the TTL of a check
let updateTTL (state : FaktaState) (checkId : string) (note : string) (status : string) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf  "check/%s/%s" status checkId)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ yield "note", Some(note) ]
  let checkUpdate = Json.serialize (CheckUpdate.GetUpdateJson status note )
  let! result = call state (agentPath (sprintf "check.%s" status)) (withJsonBody checkUpdate) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// PassTTL is used to set a TTL check to the passing state
let passTTL (state : FaktaState) (checkId : string) (note : string) : Job<Choice<unit, Error>> =
  updateTTL state checkId note "pass"

/// WarnTTL is used to set a TTL check to the warning state
let warnTTL (state : FaktaState) (checkId : string) (note : string) : Job<Choice<unit, Error>> =
  updateTTL state checkId note "warn"

/// FailTTL is used to set a TTL check to the failing state
let failTTL (state : FaktaState) (checkId : string) (note : string) : Job<Choice<unit, Error>> =
  updateTTL state checkId note "fail"

/// ForceLeave is used to have the agent eject a failed node
let forceLeave (state : FaktaState) (node : string) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "force-leave/%s" node)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentPath "force-leave") id uriBuilder HttpMethod.Put
  match result with
    | Choice1Of2 id ->
        return Choice1Of2 ()
    | Choice2Of2 err -> return Choice2Of2(err)
}