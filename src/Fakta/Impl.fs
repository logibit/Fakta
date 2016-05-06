module internal Fakta.Impl

open System
open System.Net
open System.Text
open HttpFs.Client
open NodaTime
open Fakta
open Fakta.Logging
open Chiron
open Hopac

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

  static member ofAcl (config : FaktaConfig) (op : string) =
    UriBuilder.ofModuleAndPath config "acl" op

  static member ofKVKey (config : FaktaConfig) (k : Key) =
    UriBuilder.ofModuleAndPath config "kv" k

  static member ofHealth (config : FaktaConfig) (s : string) =
    UriBuilder.ofModuleAndPath config "health" s

  static member ofEvent (config : FaktaConfig) (s : string) =
    UriBuilder.ofModuleAndPath config "event" s

  static member ofCatalog (config : FaktaConfig) (s : string) =
    UriBuilder.ofModuleAndPath config "catalog" s

  static member ofAgent (config : FaktaConfig) (a : string) =
    UriBuilder.ofModuleAndPath config "agent" a

  static member ofSession (config : FaktaConfig) (op : string) =
    UriBuilder.ofModuleAndPath config "session" op

  static member ofStatus (config : FaktaConfig) (s : string) =
    UriBuilder.ofModuleAndPath config "status" s

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

let withQueryOpts (config : FaktaConfig) (ro : QueryOptions) (req : Request) =
  // TODO: complete query options
  req

let setConfigOpts (config : FaktaConfig) (req : Request) =
  config.credentials
  |> Option.fold (fun s creds -> Request.basicAuthentication creds.username creds.password req) req

let acceptJson =
  //withHeader (Accept "application/json")
  Request.setHeader (Accept "*/*")

let withIntroductions =
  Request.setHeader (UserAgent ("Fakta " + (App.getVersion ())))

let basicRequest config meth =
  Request.create meth
  >> acceptJson
  >> withIntroductions
  >> setConfigOpts config

let withJsonBody body =
  Request.setHeader (ContentType (ContentType.Create("application", "json")))
  >> Request.bodyStringEncoded body (Encoding.UTF8)

let getResponse (state : FaktaState) path (req : Request) =
  job {
    let data = Map [ "uri", box req.url
                     "requestId", box (state.random.NextUInt64()) ]
    do! Alt.afterFun ignore << state.logger.logVerbose <| fun _ ->
      let data' = data |> Map.add "req" (box req)
      Message.create state.clock path Verbose data' "-> request"

    try
      let! res = getResponse req
      do! Alt.afterFun ignore << state.logger.logVerbose <| fun _ ->
        let data' = data |> Map.add "statusCode" (box res.statusCode)
                         |> Map.add "resp" (box res)
        Message.create state.clock path Verbose data' "<- response"

      return Choice1Of2 res
    with
    | :? System.Net.WebException as e ->
      return Choice2Of2 e
  }

let queryMeta dur (resp : Response) =
  let headerFor key = resp.headers |> Map.tryFind (ResponseHeader.NonStandard key)
  { lastIndex   = headerFor "X-Consul-Index" |> Option.fold (fun s t -> uint64 t) UInt64.MinValue
    lastContact = headerFor "X-Consul-Lastcontact" |> Option.fold (fun s t -> Duration.FromSeconds (int64 t)) Duration.Epsilon
    knownLeader = headerFor "X-Consul-Knownleader" |> Option.fold (fun s t -> Boolean.Parse(string t)) false
    requestTime = dur }

let writeMeta (dur: Duration) : (WriteMeta) =
  let res:WriteMeta = {requestTime = dur}
  res

let configOptKvs (config : FaktaConfig) : (string * string option) list =
  [ if Option.isSome config.datacenter then yield "dc", config.datacenter
    if Option.isSome config.token then yield "token", config.token ]

exception ConflictingConsistencyOptions

let private validate opts =
  opts
  |> List.filter (function ReadConsistency _ -> true | _ -> false)
  |> List.length
  |> fun n -> if n > 1 then raise ConflictingConsistencyOptions else opts

let queryOptKvs : QueryOptions -> (string * string option) list =
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

let writeCall config moduleAndOp (entity, opts : WriteOptions) =
  { inner = UriBuilder(config.serverBaseUri,
                       Path = sprintf "/%s/%s/%s" APIVersion moduleAndOp entity)
    kvs   = Map.empty }
  |> UriBuilder.mappendRange (writeOptsKvs opts)

let writeCallUri config op (entity, opts) =
  writeCall config op (entity, opts) |> UriBuilder.toUri

let call (state : FaktaState) (dottedPath : string[]) (addToReq) (uriB : UriBuilder) (httpMethod : HttpMethod) =
  let getResponse = getResponse state dottedPath
  let req =
    uriB
    |> UriBuilder.toUri
    |> basicRequest state.config httpMethod
    |> addToReq

  job {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    match resp with
    | Choice1Of2 resp ->
      use resp = resp
      if not (resp.statusCode = 200 || resp.statusCode = 404) then
        return Choice2Of2 (Message (sprintf "unknown response code %d" resp.statusCode))
      else
        match resp.statusCode with
        | 200 ->
          let! body = Response.readBodyAsString resp
          return Choice1Of2 (body, (dur, resp))
        | _ ->
          let msg = sprintf "%s error %d" (String.Join(".", dottedPath)) resp.statusCode
          return Choice.createSnd (Message msg)
    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }
