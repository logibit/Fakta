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
open Hopac

let sessionDottedPath (funcName: string) =
  [| "Fakta"; "Session"; funcName |]

let sessionPath (funcName: string) =
  [| "Fakta"; "Session"; funcName |]

let writeFilters state =
  sessionPath >> writeFilters state

let queryFilters state =
  sessionPath >> queryFilters state

let getSessionEntries (action: string) (path: string) (state : FaktaState): QueryCall<SessionEntry list> =
  let createRequest =
    queryCall state.config path

  let filters =
    queryFilters state action
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

//let getSessionEntries (value : string) (action: string) (state : FaktaState) (qo : QueryOptions)
//  : Job<Choice<SessionEntry list * QueryMeta, Error>> = job {
//  let urlPath = if value.Equals("") then (sprintf "%s" action)  else (sprintf "%s/%s" action value)
//  let uriBuilder = UriBuilder.ofSession state.config urlPath
//  let! result = call state (sessionDottedPath action) id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}


let create state: WriteCall<SessionOptions, string> =
  let writeJsonBody : SessionOptions -> Json<unit> =
      List.fold (fun acc -> function
                | LockDelay dur -> acc *> Json.write "LockDelay" (Duration.consulString dur)
                | Node node     -> acc *> Json.write "Node" node
                | Name name     -> acc *> Json.write "Name" name
                | Checks checks -> acc *> Json.write "Checks" checks
                | Behaviour sb  -> acc *> Json.write "Behavior" sb
                | TTL dur       -> acc *> Json.write "TTL" (Duration.consulString dur))
                (fun json -> Value (), Json.Object Map.empty)

  
  let createRequest (sessionOpts, opts) =
    let reqBody = snd (writeJsonBody sessionOpts (Json.Null ()))
    writeCallUri state.config "session/create" opts
    |> basicRequest state.config Put
    |> withJsonBody reqBody

  let filters =
    writeFilters state "create"
    >> respBodyFilter
    >> codec createRequest (ofJsonPrism ConsulResult.objectId)

  HttpFs.Client.getResponse |> filters
  

/// Create makes a new session. Providing a session entry can customize the
/// session. It is recommended you give a Name as the options.
//let create (state : FaktaState) (sessionOpts : SessionOptions) (opts : WriteOptions) : Job<Choice<string * WriteMeta, Error>> = job {
//  let writeJsonBody : SessionOptions -> Json<unit> =
//      List.fold (fun acc -> function
//                | LockDelay dur -> acc *> Json.write "LockDelay" (Duration.consulString dur)
//                | Node node     -> acc *> Json.write "Node" node
//                | Name name     -> acc *> Json.write "Name" name
//                | Checks checks -> acc *> Json.write "Checks" checks
//                | Behaviour sb  -> acc *> Json.write "Behavior" sb
//                | TTL dur       -> acc *> Json.write "TTL" (Duration.consulString dur))
//                (fun json -> Value (), Json.Object Map.empty)
//
//  let reqBody = snd (writeJsonBody sessionOpts (Json.Null ()))
//
//  let urlPath = "create"
//  let uriBuilder = UriBuilder.ofSession state.config urlPath
//  let! result =call state (sessionDottedPath urlPath) (withJsonBody reqBody) uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let id = Json.Object_ >?> Optics.Map.key_ "ID" >?> Json.String_
//      match Json.tryParse body with
//      | Choice1Of2 json ->
//          match Optic.get id json with
//          | Some id ->
//            return Choice1Of2 (id, { requestTime = dur })
//
//          | None ->
//            return Choice2Of2 (Message "session create: unexpected json result value")
//
//      | Choice2Of2 err ->
//        return Choice2Of2 (Message err)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let createNoChecks state: WriteCall<SessionOptions, string> =  
  let cr (so, opts) =
    let newSessionOpts =
      so
      |> List.map(fun x -> not(x.ToString().Equals("Checks")), x)
      |> List.filter fst |> List.map snd
    create state (newSessionOpts, opts)
  cr
    

/// CreateNoChecks is like Create but is used specifically to create a session with no associated health checks.
//let createNoChecks (state : FaktaState) (sessionOpts : SessionOptions) (wo : WriteOptions) : Job<Choice<string * WriteMeta, Error>> =
//  let newSessionOpts =
//    sessionOpts
//    |> List.map(fun x -> not(x.ToString().Equals("Checks")), x)
//    |> List.filter fst |> List.map snd
//  create state newSessionOpts wo

let destroy state: WriteCall<string, unit> =
  let createRequest =
    writeCallEntityUri state.config "session/destroy"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "destroy"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters  

/// Destroy invalides a given session
//let destroy (state : FaktaState) (sessionID : string) (opts : WriteOptions) : Job<Choice<WriteMeta, Error>> = job {
//  let urlPath = (sprintf "destroy/%s" sessionID)
//  let uriBuilder = UriBuilder.ofSession state.config urlPath
//                    |> UriBuilder.mappendRange (writeOptsKvs opts)
//  let! result = call state (sessionDottedPath "destroy") id uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (_, (dur, _)) ->
//      return Choice1Of2 { requestTime = dur }
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let info state: QueryCall<string, SessionEntry> =  
  let createRequest =
    queryCallEntityUri state.config "session/info"
    >> basicRequest state.config Get

  let filters =
    queryFilters state "info"
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters


/// Info looks up a single session
//let info (state : FaktaState) (sessionID : Session) (qo : QueryOptions) : Job<Choice<SessionEntry * QueryMeta, Error>> = job {
//  let urlPath = (sprintf "info/%s" sessionID )
//  let uriBuilder = UriBuilder.ofSession state.config urlPath
//  let! result = call state (sessionDottedPath "info") id uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      let item = if items.Length = 0 then SessionEntry.empty else items.[0]
//      return Choice1Of2 (item, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}


/// List gets all active sessions
let list (state : FaktaState) : QueryCall<SessionEntry list> =
  let list (qo) = 
    getSessionEntries "list" "session/list" state qo
  list
  

/// List gets sessions for a node
let node (state : FaktaState) : QueryCall<string, SessionEntry list> =
  let node (n, qo) = 
    getSessionEntries "node" ("session/node/"+n) state qo
  node


let renew state: WriteCall<string, SessionEntry> =
  let createRequest =
    fun (entity, opts) -> string entity, opts
    >> writeCallEntityUri state.config "session/renew"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "renew"
    >> respBodyFilter
    >> codec createRequest (ofJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

/// Renew renews the TTL on a given session
//let renew (state : FaktaState) (sessionID : string) (wo : WriteOptions) : Job<Choice<SessionEntry * QueryMeta, Error>> = job {
//  let urlPath = sprintf "renew/%s" sessionID
//  let uriBuilder = UriBuilder.ofSession state.config urlPath
//  let! result = call state (sessionDottedPath "renew") id uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      let item = if items.Length = 0 then SessionEntry.empty else items.[0]
//      return Choice1Of2 (item, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}


/// RenewPeriodic is used to periodically invoke Session.Renew on a session until
/// a doneCh is closed. This is meant to be used in a long running goroutine
/// to ensure a session stays valid.
let rec renewPeriodic (state : FaktaState) (ttl : Duration) (id : string) (wo : WriteOptions) (doneCh : Duration) = job {
  let! result = renew state (id, wo)
  let waitDur = ttl / int64 2
  let ms = (int)waitDur.Ticks/10000
  do! Async.Sleep(ms)
  match result with
  | Choice1Of2 entry ->
      let! res = renewPeriodic state entry.ttl id wo doneCh
      res
  | _ ->
      let! r = destroy state (id, wo)
      r |> ignore
}