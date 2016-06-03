module Fakta.KV

open System
open NodaTime
open HttpFs.Client
open Chiron
open Fakta
open Fakta.Logging
open Fakta.Impl
open Hopac

let kvPath (operation: string) =
  [| "Fakta"; "KV"; operation |]

let writeFilters state =
  kvPath >> writeFilters state

let queryFilters state =
  kvPath >> queryFilters state

////////////////////// QUERYING /////////////////////

/// Get is used to lookup a single key
let get state: QueryCall<string, KVPair> =
  let createRequest =
    queryCallEntityUri state.config "kv"
    >> basicRequest state.config Get

  let filters =
    queryFilters state "get"
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters


let getRaw (state : FaktaState) (key : Key) (opts : QueryOptions) : Job<Choice<byte [] * QueryMeta, Error>> =
  raise (TBD "TODO")

/// Keys is used to list all the keys under a prefix. Optionally, a separator can be used to limit the responses.
let keys (s : FaktaState) (key : Key) (sep : string option) (opts : QueryOptions) : Job<Choice<Keys * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List is used to lookup all keys (and their values) under a prefix
let list state : QueryCall<string, KVPairs> =
  let createRequest (prefix, qo) =
    queryCall state.config ("kv"+prefix) qo
    |> Request.queryStringItem "recurse" ""

  let filters =
    queryFilters state "list"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters


////////////////////// WRITING /////////////////////

let private mkReq methd (state : FaktaState) (kvp : KVPair) fUri (opts : WriteOptions) (body : RequestBody) =
  UriBuilder.ofKVKey state.config kvp.key
  |> UriBuilder.mappendRange (configOptKvs state.config)
  |> UriBuilder.mappendRange (writeOptsKvs opts)
  |> fUri
  |> UriBuilder.toUri
  |> basicRequest state.config methd
  |> Request.body body

let private mkPut = mkReq HttpMethod.Put
let private mkDel = mkReq HttpMethod.Delete

let private boolResponse getResponse req =
  job {
    let! response, dur = Duration.timeAsync (fun () -> getResponse req)
    match response with
    | Choice1Of2 response ->
      use response = response
      match response.statusCode with
      | 200 ->
        let! body = Response.readBodyAsString response
        match body with
        | "true" ->
          return Choice1Of2 (true, { requestTime = dur })

        | "false" ->
          return Choice1Of2 (false, { requestTime = dur })

        | x ->
          return Choice2Of2 (Message x)

      | x ->
        return Choice2Of2 (Message (sprintf "unkown status code %d for response %A" x response))


    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }

/// Acquire is used for a lock acquisiton operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
///
/// The <body> (KVPair.Value) of the PUT should be a JSON object representing
/// the local node. This value is opaque to Consul, but it should contain
/// whatever information clients require to communicate with your application
/// (e.g., it could be a JSON object that contains the node's name and the
/// application's port).
let acquire (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
  mkPut state kvp (flip UriBuilder.mappend ("acquire", kvp.session))
        opts (BodyRaw kvp.value)
  |> boolResponse (getResponse state [| "Fakta"; "KV"; "acquire" |])

/// Delete is used to delete a single key
let delete (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  mkDel state kvp (mCas |> Option.fold (fun s t ->
                                         flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                                       id)
           opts (BodyRaw [||])
  |> boolResponse (getResponse state [| "Fakta"; "KV"; "delete" |])

/// DeleteCAS is used for a Delete Check-And-Set operation. The Key and ModifyIndex are respected. Returns true on success or false on failures.
let deleteCAS (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  delete state kvp (Some kvp.modifyIndex) opts

/// DeleteTree is used to delete all keys under a prefix
let deleteTree (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  mkDel state kvp (mCas |> Option.fold (fun s t ->
                                         flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                                       id
                   >> flip UriBuilder.mappend ("recurse", None))
           opts (BodyRaw [||])
  |> boolResponse (getResponse state [|"Fakta"; "KV"; "deleteTree"|])

/// Put is used to write a new value. Only the Key, Flags and Value is respected.
let put (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  mkPut state kvp
        (mCas |> Option.fold (fun s t ->
                               flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                             id)
        opts (BodyRaw kvp.value)
  |> boolResponse (getResponse state [|"Fakta"; "KV"; "put"|])

/// CAS is used for a Check-And-Set operation. The Key, ModifyIndex, Flags and Value are respected. Returns true on success or false on failures.
let CAS (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  put state kvp (Some kvp.modifyIndex) opts

/// Release is used for a lock release operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
let release (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Job<Choice<bool * WriteMeta, Error>> =
  if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
  mkPut state kvp (flip UriBuilder.mappend ("release", kvp.session)) opts (BodyRaw [||])
  |> boolResponse (getResponse state [|"Fakta"; "KV"; "release"|])