/// https://www.consul.io/docs/internals/sessions.html
module Fakta.Session

open NodaTime
open HttpFs.Client
open Chiron
open Chiron.Operators
open Fakta
open Fakta.Impl
open Hopac

let sessionDottedPath (funcName: string) =
  [| "Fakta"; "Session"; funcName |]

let internal sessionPath (funcName: string) =
  [| "Fakta"; "Session"; funcName |]

let internal writeFilters state =
  sessionPath >> writeFilters state

let internal queryFilters state =
  sessionPath >> queryFilters state

let getSessionEntries (action: string) (path: string) (state : FaktaState): QueryCall<SessionEntry list> =
  let createRequest =
    queryCall state.config path

  let filters =
    queryFilters state action
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Create makes a new session. Providing a session entry can customize the
/// session. It is recommended you give a Name as the options.
let create state: WriteCallNoMeta<SessionOptions, string> =
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


/// CreateNoChecks is like Create but is used specifically to create a session with no associated health checks.
let createNoChecks state: WriteCallNoMeta<SessionOptions, string> =  
  let cr (so, opts) =
    let newSessionOpts =
      so
      |> List.map(fun x -> not(x.ToString().Equals("Checks")), x)
      |> List.filter fst |> List.map snd
    create state (newSessionOpts, opts)
  cr

/// Destroy invalides a given session
let destroy state: WriteCallNoMeta<string, unit> =
  let createRequest =
    writeCallEntityUri state.config "session/destroy"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "destroy"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters  

/// Info looks up a single session
let info state: QueryCall<string, SessionEntry> =  
  let createRequest =
    queryCallEntityUri state.config "session/info"
    >> basicRequest state.config Get

  let filters =
    queryFilters state "info"
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

/// List gets all active sessions
let list (state : FaktaState) : QueryCall<SessionEntry list> =
  let list (qo) = 
    getSessionEntries "list" "session/list" state qo
  list
  

/// gets sessions for a node
let node (state : FaktaState) : QueryCall<string, SessionEntry list> =
  let node (n, qo) = 
    getSessionEntries "node" ("session/node/"+n) state qo
  node

/// Renew renews the TTL on a given session
let renew state: WriteCallNoMeta<string, SessionEntry> =
  let createRequest =
    fun (entity, opts) -> string entity, opts
    >> writeCallEntityUri state.config "session/renew"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "renew"
    >> respBodyFilter
    >> codec createRequest (ofJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

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