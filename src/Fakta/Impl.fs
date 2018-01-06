module internal Fakta.Impl

open System
open System.Diagnostics
open System.Text
open HttpFs.Client
open HttpFs.Composition
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Logging.Message
open Chiron
open Hopac
open Aether.Operators


/// API version that we talk with consul
[<Literal>]
let APIVersion = "v1"

let keyFor (m : string) (k : Key) =
  let m', k' = m.Trim('/'), k.TrimStart('/') // valid to end with /
  if k = "/" then
    sprintf "/%s/%s/" APIVersion m'
  else
    sprintf "/%s/%s/%s" APIVersion m' k'

type UriBuilder =
  { inner : System.UriBuilder
    kvs   : Map<string, string option> }

  static member ofModuleAndPath (config : FaktaConfig) (``module`` : string) (path : string) =
    { inner = UriBuilder(config.serverBaseUri, Path = keyFor ``module`` path)
      kvs   = Map.empty }

  static member ofKVKey (config : FaktaConfig) (k : Key) =
    UriBuilder.ofModuleAndPath config "kv" k

  static member ofEvent (config : FaktaConfig) (k : Key) =
    UriBuilder.ofModuleAndPath config "event" k

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module UriBuilder =
  /// Build a query from the unencoded key-value pairs
  let private buildQuery =
    Map.toList
    >> List.map (fun (n, v) -> n, v |> Option.map Uri.UnescapeDataString)
    >> List.map (function
                | n, None -> n
                | n, Some ev -> String.Concat [ n; "="; ev ])
    >> String.concat "&"

  let toUri (ub : UriBuilder) =
    ub.inner.Query <- buildQuery ub.kvs
    ub.inner.Uri

  // mappend  :: Monoid a => a -> a -> a
  let mappend (ub : UriBuilder) (k, v) =
    { ub with kvs = ub.kvs |> Map.put k v }

  let mappendRange kvs (ub : UriBuilder) =
    List.fold mappend ub kvs

let setConfigOpts (config : FaktaConfig) (req : Request) =
  config.credentials
  |> Option.fold (fun s creds -> Request.basicAuthentication creds.username creds.password req) req

let acceptJson =
  //withHeader (Accept "application/json")
  Request.setHeader (Accept "*/*")

let withVaultHeader (config : FaktaConfig) =
  let token = match config.token with 
              | None -> String.Empty
              | _ -> config.token.Value.ToString()
  Request.setHeader (Custom  ("X-Vault-Token", token))

let withIntroductions =
  Request.setHeader (UserAgent ("Fakta " + (App.getVersion ())))

/// New JSON request, with Agent and config opts.
let basicRequest config meth =
  Request.create meth
  >> acceptJson
  >> withIntroductions
  >> setConfigOpts config

let withJsonBody jsonBody =
  Request.setHeader (ContentType (ContentType.create("application", "json")))
  >> Request.bodyStringEncoded (Json.format jsonBody) Encoding.UTF8

let inline withJsonBodyT value =
  Json.serialize value |> withJsonBody

let queryMeta dur (resp : Response) =
  let headerFor key = resp.headers |> Map.tryFind (ResponseHeader.NonStandard key)
  { lastIndex   = headerFor "X-Consul-Index" |> Option.fold (fun s t -> uint64 t) UInt64.MinValue
    lastContact = headerFor "X-Consul-Lastcontact" |> Option.fold (fun s t -> Duration.FromSeconds (int64 t)) Duration.Epsilon
    knownLeader = headerFor "X-Consul-Knownleader" |> Option.fold (fun s t -> Boolean.Parse(string t)) false
    requestTime = dur }

let writeMeta (dur: Duration) : WriteMeta =
  let res:WriteMeta = {requestTime = dur}
  res

let configOptKvs (config : FaktaConfig) : (string * string option) list =
  [ if Option.isSome config.datacenter then yield "dc", config.datacenter
    if Option.isSome config.token then yield "token", config.token ]

exception ConflictingConsistencyOptionsException

let private validate opts =
  opts
  |> List.filter (function ReadConsistency _ -> true | _ -> false)
  |> List.length
  |> fun n -> if n > 1 then raise ConflictingConsistencyOptionsException else opts

let queryOptsKvs : QueryOptions -> (string * string option) list =
  validate
  >> List.fold (fun acc -> function
               | ReadConsistency Default    -> acc
               | ReadConsistency Consistent -> ("consistent", None) :: acc
               | ReadConsistency Stale      -> ("stale", None) :: acc
               | Wait (index, dur) ->
                    ("index", Some (index.ToString()))
                 :: ("wait",  Some (Duration.consulString dur))
                 :: acc
               | QueryOption.TokenOverride token -> ("token", Some token) :: acc
               | QueryOption.DataCenter dc       -> ("dc", Some dc) :: acc
               )
              []

let writeOptsKvs : WriteOptions -> (string * string option) list =
  List.fold (fun acc -> function
             | WriteOption.TokenOverride token        -> ("token", Some token) :: acc
             | WriteOption.DataCenter dc              -> ("dc", Some dc) :: acc)
            []


/// For when the operation is to create or delete and there's no entity
/// parametised in the API function. Like /v1/acl/create (with body) is.
///
/// /v1/MODULE/OP?write-opts => Uri
let writeCallUri config moduleAndOp opts =
  { inner = UriBuilder(config.serverBaseUri,
                       Path = sprintf "/%s/%s" APIVersion moduleAndOp)
    kvs   = writeOptsKvs opts |> Map.ofList }
  |> UriBuilder.toUri

/// For when the entity is parametised in the API function, like
/// /v1/acl/clone/{entity} is.
///
/// /v1/MODULE/OP/ENTITY?write-opts => Uri
let writeCallEntityUri config moduleAndOp (entity, opts) =
  { inner = UriBuilder(config.serverBaseUri,
                       Path = sprintf "/%s/%s/%s" APIVersion moduleAndOp entity)
    kvs   = writeOptsKvs opts |> Map.ofList }
|> UriBuilder.toUri

let writeCall config moduleAndOp opts =
  writeCallUri config moduleAndOp opts
  |> basicRequest config Put

let writeCallEntity config moduleAndOp (entity, opts) =
  writeCallEntityUri config moduleAndOp (entity, opts)
  |> basicRequest config Put

let queryCallUri config moduleAndOp opts =
  { inner = UriBuilder(config.serverBaseUri,
                       Path = sprintf "/%s/%s" APIVersion moduleAndOp)
    kvs   = queryOptsKvs opts |> Map.ofList }
  |> UriBuilder.toUri

let queryCallEntityUri config moduleAndOp (entity, opts) =
  { inner = UriBuilder(config.serverBaseUri,
                       Path = sprintf "/%s/%s/%s" APIVersion moduleAndOp entity)
    kvs   = queryOptsKvs opts |> Map.ofList }
  |> UriBuilder.toUri

let queryCall config moduleAndOp opts =
  queryCallUri config moduleAndOp opts
  |> basicRequest config Get

let queryCallEntity config moduleAndOp (entity, opts) =
  queryCallEntityUri config moduleAndOp opts
  |> basicRequest config Get

type WriteCall<'i, 'o> = JobFunc<'i * WriteOptions, Choice<'o * WriteMeta, Error>>
type WriteCall<'o> = JobFunc<WriteOptions, Choice<'o * WriteMeta, Error>>
type WriteCallNoMeta<'i, 'o> = JobFunc<'i * WriteOptions, Choice<'o, Error>>
type WriteCallNoMeta<'o> = JobFunc<WriteOptions, Choice<'o, Error>>

type QueryCall<'i, 'o> = JobFunc<'i * QueryOptions, Choice<'o * QueryMeta, Error>>
type QueryCall<'o> = JobFunc<QueryOptions, Choice<'o * QueryMeta, Error>>
type QueryCallNoMeta<'i, 'o> = JobFunc<'i * QueryOptions, Choice<'o, Error>>
type QueryCallNoMeta<'o> = JobFunc<QueryOptions, Choice<'o, Error>>

let unknownsFilter : JobFilter<Request, Response, Request, Choice<Response, Error>> =
  fun next ->
    next >> Alt.afterFun (fun resp ->
      if not (resp.statusCode = 200 || resp.statusCode = 204 || resp.statusCode = 404) then
        Choice.createSnd (Message (sprintf "unknown response code %d" resp.statusCode))
      elif resp.statusCode = 404 then
        Choice.createSnd (Error.ResourceNotFound)
      else
        Choice.create resp
    )

let exnsFilter : JobFilter<Request, Choice<Response, Error>> =
  fun next req ->
    Alt.tryIn (next req) Job.result (function
      | :? System.Net.WebException as e ->
        Job.result (Choice.createSnd (Error.ConnectionFailed e))
      | e ->
        raise e)

let writeMetaFilter : JobFilter<'i, Choice<'o, _>, 'i, Choice<'o * WriteMeta, _>> =
  fun next req ->
    Alt.prepareFun (fun () ->
      let sw = Stopwatch.StartNew()
      next req |> Alt.afterFun (Choice.map (fun resp ->
      sw.Stop()
      resp, { requestTime = Duration.FromTicks sw.ElapsedTicks }))
    )

let respBodyFilter : JobFilter<Request, Choice<Response, Error>, Request, Choice<string, Error>> =
  fun next req ->
    next req |> Alt.afterJob (function
      | Choice1Of2 resp ->
        resp |> Response.readBodyAsString |> Job.map Choice.create

      | Choice2Of2 error ->
        Job.result (Choice.createSnd error))

let respQueryFilter : JobFilter<Request, Choice<Response, Error>, Request, Choice<string * QueryMeta, Error>> =
  fun next req ->
    Duration.timeJob (fun () -> next req) |> Alt.afterJob (function
      | Choice1Of2 resp, dur ->
        Response.readBodyAsString resp |> Job.map (fun body ->
        Choice.create (body, queryMeta dur resp))

      | Choice2Of2 error, dur ->
        Job.result (Choice.createSnd error))

let respQueryFilterNoMeta : JobFilter<Request, Choice<Response, Error>, Request, Choice<string, Error>> =
  respQueryFilter
  >> JobFunc.map (Choice.map fst)

let internal writeFilters state path =
  HttpFs.Composition.timerFilterNamed state.clientState path
  >> unknownsFilter
  >> exnsFilter

let internal queryFilters state path =
  HttpFs.Composition.timerFilterNamed state.clientState path
  >> unknownsFilter
  >> exnsFilter
  >> respQueryFilter

let internal queryFiltersNoMeta (state : FaktaState) path =
  HttpFs.Composition.timerFilterNamed state.clientState path
  >> unknownsFilter
  >> exnsFilter
  >> respQueryFilterNoMeta

let codec prepare interpret : JobFilter<'a, Choice<'b, Error>, 'i, Choice<'o, _>> =
  JobFunc.mapLeft prepare
  >> JobFunc.map (Choice.bind interpret)

let hasNoRespBody _ =
  Choice.create ()

module ConsulResult =

  let objectId =
    Json.Object_
    >?> Aether.Optics.Map.key_ "ID"

  let firstObjectOfArray =
    Json.Array_
    >?> Aether.Optics.List.head_

module VaultResult = 
  let getProperty (name: string) =
    Json.Object_
    >?> Aether.Optics.Map.key_ name

let inline ofJsonPrism jsonPrism : string -> Choice<'a, Error> =
  Json.tryParse
  >> Choice.bind (Aether.Optic.get jsonPrism >> Choice.ofOption (fun () -> "Expected property missing"))
  >> Choice.bind Json.tryDeserialize
  >> Choice.mapSnd Error.Message

/// Convert the first value in the tuple in the choice to some type 'a.
let inline internal fstOfJsonPrism jsonPrism (body, item2) : Choice< ^a * 'b, Error> =
  Json.tryParse body
  |> Choice.bind (Aether.Optic.get jsonPrism >> Choice.ofOption (fun () -> "Expected property missing"))
  |> Choice.bind Json.tryDeserialize
  |> Choice.map (fun x -> x, item2)
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg body)
  |> Choice.mapSnd Error.Message

let inline internal fstOfJsonPrismNoMeta jsonPrism body : Choice<'a, Error> =
  Json.tryParse body
  |> Choice.bind (Aether.Optic.get jsonPrism >> Choice.ofOption (fun () -> "Expected property missing"))
  |> Choice.bind Json.tryDeserialize
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg body)
  |> Choice.mapSnd Error.Message

/// Convert the first value in the tuple in the choice to some type 'a.
let inline internal fstOfJson (body, item2) : Choice< ^a * 'b, Error> =
  Json.tryParse body
  |> Choice.bind Json.tryDeserialize
  |> Choice.map (fun x -> x, item2)
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg body)
  |> Choice.mapSnd Error.Message

let inline internal fstOfJsonNoMeta body : Choice<'a, Error> =
  Json.tryParse body
  |> Choice.bind Json.tryDeserialize
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg body)
  |> Choice.mapSnd Error.Message