module Fakta.KV

open System
open NodaTime
open HttpFs.Client
open Chiron
open Fakta
open Fakta.Logging
open Fakta.Impl

////////////////////// QUERYING /////////////////////

/// Get is used to lookup a single key
let get (state : FaktaState) (key : Key) (opts : QueryOptions) : Async<Choice<KVPair * QueryMeta, Error>> =
  let getResponse = getResponse state "Fakta.KV.get"
  let req =
    UriBuilder.ofKVKey state.config key
    |> flip UriBuilder.mappendRange (queryOptKvs opts)
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
        | 404 -> return Choice2Of2 (KeyNotFound key)
        | _ ->
          let! body = Response.readBodyAsString resp
          match Json.parse body |> Json.deserialize with
          | [ x ] ->
            return Choice1Of2 (x, queryMeta dur resp)

          | xs ->
            return failwithf "unexpected case %A" xs

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }

let getRaw (state : FaktaState) (key : Key) (opts : QueryOptions) : Async<Choice<byte [] * QueryMeta, Error>> =
  raise (TBD "TODO")

/// Keys is used to list all the keys under a prefix. Optionally, a separator can be used to limit the responses.
let keys (s : FaktaState) (key : Key) (sep : string option) (opts : QueryOptions) : Async<Choice<Keys * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List is used to lookup all keys (and their values) under a prefix
let list (state : FaktaState) (prefix : Key) (opts : QueryOptions) : Async<Choice<KVPairs * QueryMeta, Error>> =
  let getResponse = getResponse state "Fakta.KV.list"
  let req =
    UriBuilder.ofKVKey state.config prefix
    |> flip UriBuilder.mappendRange [ yield! queryOptKvs opts
                                      yield "recurse", None ]
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
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)

    | Choice2Of2 exx ->
      return Choice2Of2 (Error.ConnectionFailed exx)
  }

////////////////////// WRITING /////////////////////

let private mkReq methd (state : FaktaState) (kvp : KVPair) fUri (opts : WriteOptions) (body : RequestBody) =
  UriBuilder.ofKVKey state.config kvp.key
  |> flip UriBuilder.mappendRange (configOptKvs state.config)
  |> flip UriBuilder.mappendRange (writeOptsKvs opts)
  |> fUri
  |> UriBuilder.uri
  |> basicRequest methd
  |> withConfigOpts state.config
  |> withBody body

let private mkPut = mkReq HttpMethod.Put
let private mkDel = mkReq HttpMethod.Delete

let private boolResponse getResponse req =
  async {
    let! response, dur = Duration.timeAsync (fun () -> getResponse req)
    match response with
    | Choice1Of2 response ->
      use response = response
      match response.StatusCode with
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
let acquire (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
  mkPut state kvp (flip UriBuilder.mappend ("acquire", kvp.session))
        opts (BodyRaw kvp.value)
  |> boolResponse (getResponse state "Fakta.KV.acquire")

/// Delete is used to delete a single key
let delete (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  mkDel state kvp (mCas |> Option.fold (fun s t ->
                                         flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                                       id)
           opts (BodyRaw [||])
  |> boolResponse (getResponse state "Fakta.KV.delete")

/// DeleteCAS is used for a Delete Check-And-Set operation. The Key and ModifyIndex are respected. Returns true on success or false on failures.
let deleteCAS (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  delete state kvp (Some kvp.modifyIndex) opts

/// DeleteTree is used to delete all keys under a prefix
let deleteTree (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  mkDel state kvp (mCas |> Option.fold (fun s t ->
                                         flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                                       id
                   >> flip UriBuilder.mappend ("recurse", None))
           opts (BodyRaw [||])
  |> boolResponse (getResponse state "Fakta.KV.deleteTree")

/// Put is used to write a new value. Only the Key, Flags and Value is respected.
let put (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  mkPut state kvp
        (mCas |> Option.fold (fun s t ->
                               flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                             id)
        opts (BodyRaw kvp.value)
  |> boolResponse (getResponse state "Fakta.KV.put")

/// CAS is used for a Check-And-Set operation. The Key, ModifyIndex, Flags and Value are respected. Returns true on success or false on failures.
let CAS (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  put state kvp (Some kvp.modifyIndex) opts

/// Release is used for a lock release operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
let release (state : FaktaState) (kvp : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
  mkPut state kvp (flip UriBuilder.mappend ("release", kvp.session)) opts (BodyRaw [||])
  |> boolResponse (getResponse state "Fakta.KV.release")