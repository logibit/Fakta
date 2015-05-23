/// https://www.consul.io/docs/internals/sessions.html
module Fakta.Session

open System
open NodaTime
open HttpFs.Client
open Aether
open Aether.Operators
open Chiron
open Chiron.Operators
open Fakta
open Fakta.Logging
open Fakta.Impl

/// Create makes a new session. Providing a session entry can customize the session.
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
    use resp = resp
    match resp.StatusCode with
    | 200 ->
      let! body = Response.readBodyAsString resp
      let id = Json.ObjectPLens >??> Aether.mapPLens "ID" >??> Json.StringPLens
      match Json.tryParse body with
      | Choice1Of2 json ->
        match Lens.getPartial id json with
        | Some id      -> return Choice1Of2 (id, { requestTime = dur })
        | None         -> return Choice2Of2 (Message "unexpected json result value")
      | Choice2Of2 err -> return Choice2Of2 (Message err)
    | x                -> return Choice2Of2 (Message (sprintf "unknown status code %d" x))
  }

/// CreateNoChecks is like Create but is used specifically to create a session with no associated health checks. 
let createNoChecks (state : FaktaState) (sessionOpts : SessionOptions) (wo : WriteOptions) : Async<Choice<Session * WriteMeta, Error>> =
  raise (TBD "TODO")

/// Destroy invalides a given session
let destroy (state : FaktaState) (session : Session) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  let getResponse = getResponse state "Fakta.Session.destroy"
  
  let req =
    UriBuilder.ofSession state.config (sprintf "destroy/%s" session)
    |> flip UriBuilder.mappendRange (writeOptsKvs opts)
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config

  async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    use resp = resp
    match resp.StatusCode with
    | 200 -> return Choice1Of2 { requestTime = dur }
    | x   -> return Choice2Of2 (Message (sprintf "unkown status code %d" x))
  }


/// Info looks up a single session 
let info (state : FaktaState) (id : Session) (qo : QueryOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List gets all active sessions 
let list (state : FaktaState) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List gets sessions for a node
let node (s : FaktaState) (node : Node) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  raise (TBD "TODO")

/// Renew renews the TTL on a given session
let renew (s : FaktaState) (id : Session) (wo : WriteOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
  raise (TBD "TODO")

/// RenewPeriodic is used to periodically invoke Session.Renew on a session until
/// a doneCh is closed. This is meant to be used in a long running goroutine
/// to ensure a session stays valid. 
let renewPeriodic (s : FaktaState) (initialTTL : Duration) (id : string) (wo : WriteOptions) : Async<Choice<unit, Error>> =
  //func (s *Session) RenewPeriodic(initialTTL string, id string, q *WriteOptions, doneCh chan struct{}) error
  raise (TBD "TODO")