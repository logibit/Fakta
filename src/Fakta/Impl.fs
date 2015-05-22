module internal Fakta.Impl

open System
open System.Net
open HttpFs.Client
open NodaTime
open Fakta
open Fakta.Logging

let Namespace = "/v1/kv"

let keyFor (k : Key) =
  if k = "/" then Namespace + "/" else
  String.concat "/" [ yield Namespace; yield k.TrimStart('/') ]

type UriBuilder =
  { inner : System.UriBuilder
    kvs   : (string * string option) list }

  static member ofKVKey (config : FaktaConfig) (k : Key) =
    let uri = config.serverBaseUris.Head
    let uriBuilder = System.UriBuilder uri
    uriBuilder.Path <- keyFor k
    { inner = uriBuilder
      kvs   = [] }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module UriBuilder =
  /// Build a query from the unencoded key-value pairs
  let private buildQuery =
    List.map (fun (n, v) -> n, v |> Option.map Uri.EscapeUriString)
    >> List.map (function
                | n, None -> n
                | n, Some ev -> String.Concat [ n; "="; ev ])
    >> String.concat "&"

  let uri (ub : UriBuilder) =
    ub.inner.Query <- buildQuery ub.kvs
    ub.inner.Uri

  // mappend  :: Monoid a => a -> a -> a
  let mappend (ub : UriBuilder) (k, v) =
    { ub with kvs = (k, v) :: ub.kvs }

  let mappendRange (ub : UriBuilder) kvs =
    List.fold mappend ub kvs

let withQueryOpts (config : FaktaConfig) (ro : QueryOptions) (req : Request) =
  req

let withWriteOpts (config : FaktaConfig) (wo : WriteOptions) (req : Request) =
  config.credentials
  |> Option.fold (fun s creds -> withBasicAuthentication creds.username creds.password req) req
   
let acceptJson =
  withHeader (Accept "application/json")

let withIntroductions =
  withHeader (UserAgent "Fakta 0.1")

let getResponse (state : FaktaState) path (req : Request) =
  async {
    let data = Map [ "uri", box req.Url
                     "requestId", box (state.random.NextUInt64()) ]
    state.logger.Verbose <| fun _ ->
      let data' = data |> Map.add "req" (box req)
      LogLine.mk state.clock path Verbose data' "-> request"

    let! res = getResponse req

    state.logger.Verbose <| fun _ ->
      let data' = data |> Map.add "statusCode" (box res.StatusCode)
                       |> Map.add "resp" (box res)
      LogLine.mk state.clock path Verbose data' "<- response"

    return res
  }

let queryMeta dur (resp : Response) =
  let headerFor key = resp.Headers |> Map.find (ResponseHeader.NonStandard key)
  { lastIndex   = uint64 (headerFor "X-Consul-Index")
    lastContact = Duration.FromSeconds (int64 (headerFor "X-Consul-Lastcontact"))
    knownLeader = bool.Parse (headerFor "X-Consul-Knownleader")
    requestTime = dur }

let withBodyValueField (value : byte []) =
  withBody (
    BodyForm [ NameValue { name = "value"
                           value = Convert.ToBase64String value } ]
  )