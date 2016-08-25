module Fakta.KV

open HttpFs.Client
open HttpFs.Composition
open Fakta
open Fakta.Impl
open Hopac

let internal kvPath (operation: string) =
  [| "Fakta"; "KV"; operation |]

let internal writeFilters state =
  kvPath >> writeFilters state

let internal queryFilters state =
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

  getResponse |> filters

/// List is used to lookup all keys (and their values) under a prefix
let list state : QueryCall<string, KVPairs> =
  let createRequest (prefix, qo) =
    queryCall state.config ("kv"+prefix) qo
    |> Request.queryStringItem "recurse" ""

  let filters =
    queryFilters state "list"
    >> codec createRequest fstOfJson

  getResponse |> filters


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

let private only200s : JobFilter<_, _, _, _> =
  fun next ->
    next >> Alt.afterFun (fun resp ->
      if not (resp.statusCode = 200) then
        Choice.createSnd (Message (sprintf "Unknown response code %d" resp.statusCode))
      else
        Choice.create resp)

let internal eventPath (operation: string) =
  [| "Fakta"; "KV"; operation |]

let private boolFilter state createRequest path  =
  timerFilterNamed state.clientState (eventPath path)
  >> only200s
  >> exnsFilter
  >> respBodyFilter
  >> writeMetaFilter
  >> codec createRequest fstOfJson

/// Acquire is used for a lock acquisiton operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
///
/// The <body> (KVPair.Value) of the PUT should be a JSON object representing
/// the local node. This value is opaque to Consul, but it should contain
/// whatever information clients require to communicate with your application
/// (e.g., it could be a JSON object that contains the node's name and the
/// application's port).
let acquire (state : FaktaState) : WriteCall<KVPair, bool> =
  let createRequest (kvp : KVPair, opts) =
    if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
    mkPut state kvp (flip UriBuilder.mappend ("acquire", kvp.session))
          opts (BodyRaw kvp.value)

  getResponse |> boolFilter state createRequest "acquire"

/// Delete is used to delete a single key
let delete (state : FaktaState) : WriteCall<KVPair * Index option, bool> =
  let createRequest ((kvp : KVPair, mCas : Index option), opts : WriteOptions) =
    mkDel state kvp (mCas |> Option.fold (fun s t ->
                      flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                      id)
            opts (BodyRaw [||])

  getResponse |> boolFilter state createRequest "delete"

/// DeleteCAS is used for a Delete Check-And-Set operation. The Key and
/// ModifyIndex are respected. Returns true on success or false on failures.
let deleteCAS (state : FaktaState) : WriteCall<KVPair, bool>=
  delete state
  |> JobFunc.mapLeft (fun (kvpair, opts) -> (kvpair, Some kvpair.modifyIndex), opts)

/// DeleteTree is used to delete all keys under a prefix
let deleteTree (state : FaktaState) : WriteCall<KVPair * Index option, bool> =
  let createRequest ((kvp : KVPair, mCas : Index option), opts : WriteOptions) =
    mkDel state kvp (mCas |> Option.fold (fun s t ->
                                          flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                                        id
                    >> flip UriBuilder.mappend ("recurse", None))
            opts (BodyRaw [||])

  getResponse |> boolFilter state createRequest "deleteTree"

/// Put is used to write a new value. Only the Key, Flags and Value is respected.
let put (state : FaktaState) : WriteCall<KVPair * Index option, bool> =
  let createRequest ((kvp : KVPair, mCas : Index option), opts : WriteOptions) =
    mkPut state kvp
          (mCas |> Option.fold (fun s t ->
                                flip UriBuilder.mappend ("cas", (Some (t.ToString()))))
                              id)
          opts (BodyRaw kvp.value)

  getResponse |> boolFilter state createRequest "put"

/// CAS is used for a Check-And-Set operation. The Key, ModifyIndex, Flags and Value are respected. Returns true on success or false on failures.
let CAS (state : FaktaState) =
  put state
  |> JobFunc.mapLeft (fun (kvp, opts) -> (kvp, Some kvp.modifyIndex), opts)

/// Release is used for a lock release operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
let release (state : FaktaState) : WriteCall<KVPair, bool> =
  let createRequest (kvp : KVPair, opts) =
    if Option.isNone kvp.session then invalidArg "kvp.session" "kvp.session needs to be a value"
    mkPut state kvp (flip UriBuilder.mappend ("release", kvp.session)) opts (BodyRaw [||])
  getResponse |> boolFilter state createRequest "release"