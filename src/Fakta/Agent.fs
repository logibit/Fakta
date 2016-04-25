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

/// CheckDeregister is used to deregister a check with the local agent
let checkDeregister (state : FaktaState) (checkId : string) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.checkDeregister"
    let req =
      UriBuilder.ofAgent state.config (sprintf "check/deregister/%s" checkId)
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.checkDeregister error %d" resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// CheckRegister is used to register a new check with the local agent 
let checkRegister (state : FaktaState) (checkRegistration : AgentCheckRegistration) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.checkRegister"
    let serializedCheckReg = Json.serialize checkRegistration |> Json.format
    let req =
      UriBuilder.ofAgent state.config "check/register"
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Put
      |> withConfigOpts state.config
      |> withJsonBody serializedCheckReg
    async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
        return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
      else
        match resp.StatusCode with      
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.checkRegister error %d" resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
    }

/// Checks returns the locally registered checks
let checks (state : FaktaState) : Async<Choice<Map<string, AgentCheck>, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.checks"
    let req =
      UriBuilder.ofAgent state.config "checks" 
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
  
let setNodeMaintenanceMode (state : FaktaState) (enable : bool) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.maintenance"
    let req =
      UriBuilder.ofAgent state.config "maintenance"
      |> flip UriBuilder.mappendRange [ yield "enable", Some((enable.ToString().ToLower())) ]
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Put   
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.maintenance set %b error: %d" enable resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
    }

let setServiceMaintenanceMode (state : FaktaState) (enable : bool) (serviceId : string) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.service.maintenance"
    let req =
      UriBuilder.ofAgent state.config (sprintf "service/maintenance/%s" serviceId)
      |> flip UriBuilder.mappendRange [ yield "enable", Some((enable.ToString().ToLower())) ]
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Put
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.maintenance set %b error: %d" enable resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// DisableNodeMaintenance toggles node maintenance mode off for the agent we are connected to.
let disableNodeMaintenance (state : FaktaState) : Async<Choice<unit, Error>> =
    setNodeMaintenanceMode state false

/// DisableServiceMaintenance toggles service maintenance mode off for the given service id.
let disableServiceMaintenance (state : FaktaState) (serviceId : Id) : Async<Choice<unit, Error>> =
    setServiceMaintenanceMode state false (serviceId.ToString())

/// EnableNodeMaintenance toggles node maintenance mode on for the agent we are connected to.
let enableNodeMaintenance (state : FaktaState) (reason : string) : Async<Choice<unit, Error>> =
    setNodeMaintenanceMode state true

/// EnableServiceMaintenance toggles service maintenance mode on for the given service id.
let enableServiceMaintenance (state : FaktaState) (serviceId : Id) (reason : string) : Async<Choice<unit, Error>> =
    setServiceMaintenanceMode state true (serviceId.ToString())

/// Join is used to instruct the agent to attempt a join to another cluster member
let join (state : FaktaState) (addr : string) (wan : bool) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.join"
    let req =
      UriBuilder.ofAgent state.config (sprintf "join/%s" addr)
      |> flip UriBuilder.mappendRange [ yield "wan", Some(System.Convert.ToInt16(wan).ToString()) ]
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Get
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.maintenance set %s error: %d" addr resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Members returns the known gossip members. The WAN flag can be used to query a server for WAN members.
let members (state : FaktaState) (wan : bool) : Async<Choice<AgentMember list, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.members"
    let req =
      UriBuilder.ofAgent state.config "members" 
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
        | 404 -> return Choice2Of2 (Message (sprintf "%d not found: agent.members" resp.StatusCode))
        | _ ->
          let! body = Response.readBodyAsString resp
          let  item = if body = "" then [] else Json.deserialize (Json.parse body)
          return Choice1Of2 (item)

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Self is used to query the agent we are speaking to for information about itself
let self (state : FaktaState) : Async<Choice<Map<string, Map<string, Chiron.Json>>, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.self"
    let req =
      UriBuilder.ofAgent state.config "self" 
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
        | 404 -> return Choice2Of2 (Message "agent.self not found")
        | _ ->
          let! body = Response.readBodyAsString resp
          let  item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
          return Choice1Of2 (item)

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// NodeName is used to get the node name of the agent
let nodeName (state : FaktaState) : Async<Choice<string, Error>> =
    let result = Async.RunSynchronously (self state)
    async {    
      match result with
      | Choice1Of2 map -> 
          let r, err = match map.TryFind("Config") with
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
let serviceDeregister (state : FaktaState) (serviceId : Id) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.service.deregister"
    let req =
      UriBuilder.ofAgent state.config (sprintf "check/deregister/%s" serviceId)
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.service.deregister value %s error: %d" serviceId resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
}

/// ServiceRegister is used to register a new service with the local agent
let serviceRegister (state : FaktaState) (service : AgentServiceRegistration) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.service.register"
    let serializedCheckReg = Json.serialize service |> Json.format
    let req =
      UriBuilder.ofAgent state.config "service/register"
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Put
      |> withConfigOpts state.config
      |> withJsonBody serializedCheckReg
    async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
        return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
      else
        match resp.StatusCode with      
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.service.register set %s error: %d" service.Name resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
    }

/// Services returns the locally registered services
let services (state : FaktaState) : Async<Choice<Map<string, AgentService>, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.services"
    let req =
      UriBuilder.ofAgent state.config "services" 
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
    let getResponse = Impl.getResponse state (sprintf "Fakta.Agent.check.%s" status)
    let checkUpdate = Json.serialize (CheckUpdate.GetUpdateJson status note ) |> Json.format
    let req =
      UriBuilder.ofAgent state.config (sprintf "check/%s/%s" status checkId)
      |> flip UriBuilder.mappendRange [ yield "note", Some(note) ]
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Put
      |> withConfigOpts state.config
      |> withJsonBody checkUpdate
    async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
        return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
      else
        match resp.StatusCode with      
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.check.update status %s error: %d" status resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
    }

/// PassTTL is used to set a TTL check to the passing state
let passTTL (state : FaktaState) (checkId : string) (note : string) : Async<Choice<unit, Error>> =
    updateTTL state checkId note "pass"

/// WarnTTL is used to set a TTL check to the warning state
let warnTTL (state : FaktaState) (checkId : string) (note : string) : Async<Choice<unit, Error>> =
    updateTTL state checkId note "warn"

/// FailTTL is used to set a TTL check to the failing state
let failTTL (state : FaktaState) (checkId : string) (note : string) : Async<Choice<unit, Error>> =
    updateTTL state checkId note "fail"

/// ForceLeave is used to have the agent eject a failed node
let forceLeave (state : FaktaState) (node : string) : Async<Choice<unit, Error>> =
    let getResponse = Impl.getResponse state "Fakta.Agent.force-leave"
    let req =
      UriBuilder.ofAgent state.config (sprintf "force-leave/%s" node)
      |> UriBuilder.uri
      |> basicRequest HttpMethod.Get
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
        | 200 -> return Choice1Of2 ()
        | _ ->  return Choice2Of2 (Message (sprintf "agent.force-leave node %s error: %d" node resp.StatusCode))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
    }