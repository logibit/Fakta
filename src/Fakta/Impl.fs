﻿module internal Fakta.Impl

open System
open System.Net
open System.Text
open HttpFs.Client
open NodaTime
open Fakta
open Fakta.Logging
open Chiron

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

  static member ofModuleAndPath (config : FaktaConfig) (mdle : string) (path : string) =
    { inner = UriBuilder(config.serverBaseUri, Path = keyFor mdle path)
      kvs   = Map.empty }

  static member ofAcl (config : FaktaConfig) (s : string) =
    UriBuilder.ofModuleAndPath config "acl" s

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

  let uri (ub : UriBuilder) =
    ub.inner.Query <- buildQuery ub.kvs
    ub.inner.Uri

  // mappend  :: Monoid a => a -> a -> a
  let mappend (ub : UriBuilder) (k, v) =
    { ub with kvs = ub.kvs |> Map.put k v }

  let mappendRange (ub : UriBuilder) kvs =
    List.fold mappend ub kvs

let withQueryOpts (config : FaktaConfig) (ro : QueryOptions) (req : Request) =
  // TODO: complete query options
  req

let withConfigOpts (config : FaktaConfig) (req : Request) =
  config.credentials
  |> Option.fold (fun s creds -> withBasicAuthentication creds.username creds.password req) req

let acceptJson =
  //withHeader (Accept "application/json")
  withHeader (Accept "*/*")

let withIntroductions =
  withHeader (UserAgent "Fakta 0.1")

let basicRequest meth =
  createRequest meth
  >> acceptJson
  >> withIntroductions

let withJsonBody body =
  withHeader (ContentType (ContentType.Create("application", "json")))
  >> withBodyStringEncoded body (Encoding.UTF8)

let getResponse (state : FaktaState) path (req : Request) =
  async {
    let data = Map [ "uri", box req.Url
                     "requestId", box (state.random.NextUInt64()) ]
    state.logger.Verbose <| fun _ ->
      let data' = data |> Map.add "req" (box req)
      LogLine.mk state.clock path Verbose data' "-> request"

    try
      let! res = getResponse req
      state.logger.Verbose <| fun _ ->
        let data' = data |> Map.add "statusCode" (box res.StatusCode)
                         |> Map.add "resp" (box res)
        LogLine.mk state.clock path Verbose data' "<- response"

      return Choice1Of2 res
    with
    | :? System.Net.WebException as e ->
      return Choice2Of2 e
  }

let queryMeta dur (resp : Response) =
  let headerFor key = resp.Headers |> Map.tryFind (ResponseHeader.NonStandard key)
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

let call (state : FaktaState) (dottedPath:string) (addToReq) (uriB: UriBuilder) (httpMethod: HttpMethod) =
  let getResponse = getResponse state dottedPath
  let req =
    uriB
    |> UriBuilder.uri
    |> basicRequest httpMethod
    |> withConfigOpts state.config
    |> addToReq
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with      
      | 200 -> 
        let! body = Response.readBodyAsString resp
        return Choice1Of2 (body,(dur, resp))
      | _ ->  return Choice2Of2 (Message (sprintf "%s error %d" dottedPath resp.StatusCode))
  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
  }
  