/// https://www.consul.io/docs/internals/sessions.html
module Fakta.Session

open System
open System.Globalization
open NodaTime
open HttpFs.Client
open Aether
open Aether.Operators
open Chiron
open Chiron.Operators
open Fakta
open Fakta.Logging
open Fakta.Impl

let getSessionEntry (value : string) (action: string) (state : FaktaState) (id : Session) (qo : QueryOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state (sprintf "Fakta.session.%s" action)
  let path = if value.Equals("") then (sprintf "%s" action)  else (sprintf "%s/%s" action value)
  let req =
    UriBuilder.ofSession state.config path
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
      | 404 -> return Choice2Of2 (Message (sprintf "%s not found" action))
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        let item = if items.Length = 0 then SessionEntry.empty else items.[0]
        return Choice1Of2 (item, queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

let getSessionEntries (value : string) (action: string) (state : FaktaState) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state (sprintf "Fakta.session.%s" action)
  let path = if value.Equals("") then (sprintf "%s" action)  else (sprintf "%s/%s" action value)
  let req =
    UriBuilder.ofSession state.config path
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
      | 404 -> return Choice2Of2 (Message "Session list not found")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Create makes a new session. Providing a session entry can customize the
/// session. It is recommended you give a Name as the options.
let create (state : FaktaState) (sessionOpts : SessionOptions) (opts : WriteOptions) : Async<Choice<Session * WriteMeta, Error>> =
  let getResponse = getResponse state "Fakta.Session.create"

  let writeJsonBody : SessionOptions -> Json<unit> =
    List.fold (fun acc -> function
              | LockDelay dur -> acc *> Json.write "LockDelay" (Duration.consulString dur)
              | Node node     -> acc *> Json.write "Node" node
              | Name name     -> acc *> Json.write "Name" name
              | Checks checks -> acc *> Json.write "Checks" checks
              | Behaviour sb  -> acc *> Json.write "Behavior" sb
              | TTL dur       -> acc *> Json.write "TTL" (Duration.consulString dur))
              (fun json -> Value (), Json.Object Map.empty)

  let reqBody = Json.format (snd (writeJsonBody sessionOpts (Json.Null ())))

  let req =
    UriBuilder.ofSession state.config "create"
    |> flip UriBuilder.mappendRange (writeOptsKvs opts)
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config
    |> withJsonBody reqBody

  async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      match resp.StatusCode with
      | 200 ->
        let! body = Response.readBodyAsString resp
        let id = Json.Object_ >?> Optics.Map.key_ "ID" >?> Json.String_
        match Json.tryParse body with
        | Choice1Of2 json ->
          match Optic.get id json with
          | Some id ->
            return Choice1Of2 (id, { requestTime = dur })

          | None ->
            return Choice2Of2 (Message "session create: unexpected json result value")

        | Choice2Of2 err ->
          return Choice2Of2 (Message err)

      | x ->
        return Choice2Of2 (Message (sprintf "unknown status code %d" x))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }

/// CreateNoChecks is like Create but is used specifically to create a session with no associated health checks. 
let createNoChecks (state : FaktaState) (sessionOpts : SessionOptions) (wo : WriteOptions) : Async<Choice<Session * WriteMeta, Error>> =
  let newSessionOpts =  
    sessionOpts
    |> List.map(fun x -> not(x.ToString().Equals("Checks")), x)
    |> List.filter fst |> List.map snd
  create state newSessionOpts wo

/// Destroy invalides a given session
let destroy (state : FaktaState) (id : string) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  let getResponse = getResponse state "Fakta.Session.destroy"
  
  let req =
    UriBuilder.ofSession state.config (sprintf "destroy/%s" id)
    |> flip UriBuilder.mappendRange (writeOptsKvs opts)
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config

  async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      match resp.StatusCode with
      | 200 ->
        return Choice1Of2 { requestTime = dur }

      | x ->
        return Choice2Of2 (Message (sprintf "unkown status code %d" x))

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }


/// Info looks up a single session 
let info (state : FaktaState) (id : Session) (qo : QueryOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
    let getResponse = Impl.getResponse state "Fakta.session.info"
    let req =
      UriBuilder.ofSession state.config (sprintf "info/%s" id )
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
        | 404 -> return Choice2Of2 (Message "Session info not found")
        | _ ->
          let! body = Response.readBodyAsString resp
          let items = if body = "" then [] else Json.deserialize (Json.parse body)
          let item = if items.Length = 0 then SessionEntry.empty else items.[0]
          return Choice1Of2 (item, queryMeta dur resp)

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }
    

/// List gets all active sessions 
let list (state : FaktaState) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  getSessionEntries "" "list" state qo 

/// List gets sessions for a node
let node (state : FaktaState) (node : string) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  getSessionEntries node "node" state qo

/// Renew renews the TTL on a given session
let renew (state : FaktaState) (id : string) (wo : WriteOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
    let getResponse = Impl.getResponse state "Fakta.session.renew"
    let req =
      UriBuilder.ofSession state.config (sprintf "renew/%s" id )
      |> UriBuilder.uri
      |> basicRequest Put
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
        | 404 -> return Choice2Of2 (Message "Session renew not found")
        | _ ->
          let! body = Response.readBodyAsString resp
          let items = if body = "" then [] else Json.deserialize (Json.parse body)
          let item = if items.Length = 0 then SessionEntry.empty else items.[0]
          return Choice1Of2 (item, queryMeta dur resp)

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }

/// RenewPeriodic is used to periodically invoke Session.Renew on a session until
/// a doneCh is closed. This is meant to be used in a long running goroutine
/// to ensure a session stays valid. 
let rec renewPeriodic (state : FaktaState) (ttl : Duration) (id : string) (wo : WriteOptions) (doneCh : Duration) : Async<Choice<unit, Error>> =
  //func (s *Session) RenewPeriodic(initialTTL string, id string, q *WriteOptions, doneCh chan struct{}) error
  async {
    let waitDur = (ttl / (int64)2)
    let lastRenew = DateTime.Now.Ticks

    if (DateTime.Now.Ticks - lastRenew) > ttl.Ticks 
      then return Choice2Of2 (Message "Session expired")
      else 
        match DateTime.Now.Ticks > doneCh.Ticks with
        | true ->
           match Async.RunSynchronously (renew state id wo) with
            | Choice1Of2 result ->     
                let (entry, qo) = result
                if entry = SessionEntry.empty
                  then return Choice2Of2 (Message "Session expired") 
                  else  
                    renewPeriodic state entry.ttl id wo doneCh |> ignore
                    return Choice1Of2 ()
            | Choice2Of2 err ->  return Choice2Of2 (Message "Renew")
        | false -> 
          destroy state id wo |> ignore
          return Choice1Of2()
  }
      