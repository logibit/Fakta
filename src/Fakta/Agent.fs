/// Agent can be used to query the Agent endpoints
module Fakta.Agent
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open NodaTime
open HttpFs.Client
open Chiron

/// CheckDeregister is used to deregister a check with the local agent
let checkDeregister (state : FaktaState) (checkId : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// CheckRegister is used to register a new check with the local agent 
let checkRegister (state : FaktaState) (checkId : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// Checks returns the locally registered checks
let checks (state : FaktaState) : Async<Choice<Map<string, AgentCheck>, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Agent.checks"
  let req =
    UriBuilder.ofAgent state.config "checks" 
    |> flip UriBuilder.mappendRange (queryOptKvs [])
    |> UriBuilder.uri
    |> basicRequest Get
    |> withConfigOpts state.config
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "agent.Checks")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
        return Choice1Of2 (item)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// DisableNodeMaintenance toggles node maintenance mode off for the agent we are connected to.
let disableNodeMaintenance (state : FaktaState) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// DisableServiceMaintenance toggles service maintenance mode off for the given service id.
let disableServiceMaintenance (state : FaktaState) (serviceId : Id) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// EnableNodeMaintenance toggles node maintenance mode on for the agent we are connected to.
let enableNodeMaintenance (state : FaktaState) (reason : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// EnableServiceMaintenance toggles service maintenance mode on for the given service id.
let enableServiceMaintenance (state : FaktaState) (serviceId : Id) (reason : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// FailTTL is used to set a TTL check to the failing state
let failTTL (state : FaktaState) (checkId : string) (note : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// ForceLeave is used to have the agent eject a failed node
let forceLeave (state : FaktaState) (node : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// Join is used to instruct the agent to attempt a join to another cluster member
let join (state : FaktaState) (addr : string) (wan : bool) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// Members returns the known gossip members. The WAN flag can be used to query a server for WAN members.
let members (state : FaktaState) (wan : bool) : Async<Choice<AgentMember list, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Agent.members"
  let req =
    UriBuilder.ofAgent state.config "members" 
    |> flip UriBuilder.mappendRange (queryOptKvs [])
    |> UriBuilder.uri
    |> basicRequest Get
    |> withConfigOpts state.config
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "agent.members")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  item = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (item)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// NodeName is used to get the node name of the agent
let nodeName (state : FaktaState) : Async<Choice<string, Error>> =
  raise (TBD "TODO")

/// PassTTL is used to set a TTL check to the passing state
let passTTL (state : FaktaState) (checkId : string) (note : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// Self is used to query the agent we are speaking to for information about itself
let self (state : FaktaState) : Async<Choice<Map<string, Map<string, Chiron.Json>>, Error>> =
  raise (TBD "TODO")

/// ServiceDeregister is used to deregister a service with the local agent
let serviceDeregister (state : FaktaState) (serviceId : Id) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// ServiceRegister is used to register a new service with the local agent
let serviceRegister (state : FaktaState) (service : AgentServiceRegistration) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")


let getMap (key : string) (agents : AgentService list) : Map<string, AgentService> =
  let res =
    agents
    |> List.map (fun x -> (key, x))
    |> Map.ofList
  res

/// Services returns the locally registered services
let services (state : FaktaState) : Async<Choice<Map<string, AgentService>, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Agent.services"
  let req =
    UriBuilder.ofAgent state.config "services" 
    |> flip UriBuilder.mappendRange (queryOptKvs [])
    |> UriBuilder.uri
    |> basicRequest Get
    |> withConfigOpts state.config
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "agent.services")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  item:Map<string,AgentService> = if body = "" then Map.empty else Json.deserialize (Json.parse body)
        return Choice1Of2 (item)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// UpdateTTL is used to update the TTL of a check
let updateTTL (state : FaktaState) (checkId : string) (note : string) (status : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// WarnTTL is used to set a TTL check to the warning state
let warnTTL (state : FaktaState) (checkId : string) (note : string) (status : string) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")