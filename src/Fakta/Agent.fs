/// Agent can be used to query the Agent endpoints
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

let agentDottedPath (funcName: string) =
  [| "Fakta"; "Agent"; funcName |]

/// CheckDeregister is used to deregister a check with the local agent
let checkDeregister (state : FaktaState) (checkId : string) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "check/deregister/%s" checkId)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentDottedPath "checkDeregister") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id -> 
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// CheckRegister is used to register a new check with the local agent
let checkRegister (state : FaktaState) (checkRegistration : AgentCheckRegistration) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "check/register")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let serializedCheckReg = Json.serialize checkRegistration
  let! result = call state (agentDottedPath "checkRegister") (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Checks returns the locally registered checks
let checks (state : FaktaState) : Job<Choice<Map<string, AgentCheck>, Error>> = job {
  let urlPath = (sprintf "checks")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let  item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}


let setNodeMaintenanceMode (state : FaktaState) (enable : bool) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "maintenance")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ if enable then yield "enable", Some "true" else yield "enable", Some "false" ]
  let! result = call state (agentDottedPath urlPath) id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


let setServiceMaintenanceMode (state : FaktaState) (enable : bool) (serviceId : string) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "service/maintenance/%s" serviceId)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ yield "enable", Some((enable.ToString().ToLower())) ]
  let! result = call state (agentDottedPath "service.maintenance") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// DisableNodeMaintenance toggles node maintenance mode off for the agent we are connected to.
let disableNodeMaintenance (state : FaktaState) : Job<Choice<unit, Error>> =
  setNodeMaintenanceMode state false

/// DisableServiceMaintenance toggles service maintenance mode off for the given service id.
let disableServiceMaintenance (state : FaktaState) (serviceId : Id) : Job<Choice<unit, Error>> =
  setServiceMaintenanceMode state false (serviceId.ToString())

/// EnableNodeMaintenance toggles node maintenance mode on for the agent we are connected to.
let enableNodeMaintenance (state : FaktaState) (reason : string) : Job<Choice<unit, Error>> =
  setNodeMaintenanceMode state true

/// EnableServiceMaintenance toggles service maintenance mode on for the given service id.
let enableServiceMaintenance (state : FaktaState) (serviceId : Id) (reason : string) : Job<Choice<unit, Error>> =
  setServiceMaintenanceMode state true (serviceId.ToString())

/// Join is used to instruct the agent to attempt a join to another cluster member
let join (state : FaktaState) (addr : string) (wan : bool) : Job<Choice<unit, Error>> = job {
  let urlPath = (sprintf "join/%s" addr)
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
                    |> UriBuilder.mappendRange [ yield "wan", if wan then Some("1") else Some("0") ]
  let! result = call state (agentDottedPath "join") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Members returns the known gossip members. The WAN flag can be used to query a server for WAN members.
let members (state : FaktaState) (wan : bool) : Job<Choice<AgentMember list, Error>> = job {
  let urlPath = (sprintf "members")
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentDottedPath urlPath) id uriBuilder HttpMethod.Get
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
  let! result = call state (agentDottedPath urlPath) id uriBuilder HttpMethod.Get
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
  let! result = call state (agentDottedPath "service.deregister") id uriBuilder HttpMethod.Put
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
  let! result = call state (agentDottedPath "service.register") (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 id ->
      return Choice1Of2 ()
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Services returns the locally registered services
let services (state : FaktaState) : Job<Choice<Map<string, AgentService>, Error>> = job {
  let urlPath = "services"
  let uriBuilder = UriBuilder.ofAgent state.config urlPath
  let! result = call state (agentDottedPath urlPath) id uriBuilder HttpMethod.Get
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
  let! result = call state (agentDottedPath (sprintf "check.%s" status)) (withJsonBody checkUpdate) uriBuilder HttpMethod.Put
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
  let! result = call state (agentDottedPath "force-leave") id uriBuilder HttpMethod.Put
  match result with
    | Choice1Of2 id ->
        return Choice1Of2 ()
    | Choice2Of2 err -> return Choice2Of2(err)
}