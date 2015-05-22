module Fakta.KV

open System
open NodaTime
open HttpFs.Client
open Chiron
open Fakta
open Fakta.Logging
open Fakta.Impl

/// Acquire is used for a lock acquisiiton operation. The Key, Flags, Value and Session are respected. Returns true on success or false on failures. 
let acquire (s : FaktaState) (p : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  raise (TBD "TODO")

/// CAS is used for a Check-And-Set operation. The Key, ModifyIndex, Flags and Value are respected. Returns true on success or false on failures. 
let CAS (s : FaktaState) (p : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  raise (TBD "TODO")

/// Delete is used to delete a single key 
let delete (s : FaktaState) (p : KVPair) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "TODO")

/// DeleteCAS is used for a Delete Check-And-Set operation. The Key and ModifyIndex are respected. Returns true on success or false on failures. 
let deleteCAS (s : FaktaState) (p : KVPair) (opts : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  raise (TBD "TODO")

/// DeleteTree is used to delete all keys under a prefix
let deleteTree (s : FaktaState) (p : KVPair) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "TODO")

/// Get is used to lookup a single key 
let get (s : FaktaState) (key : Key) (raw : bool) (opts : QueryOptions) : Async<Choice<KVPair * QueryMeta, Error>> =
  raise (TBD "TODO")

/// Keys is used to list all the keys under a prefix. Optionally, a separator can be used to limit the responses. 
let keys (s : FaktaState) (key : Key) (sep : string option) (opts : QueryOptions) : Async<Choice<Keys * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List is used to lookup all keys (and their values) under a prefix
let list (state : FaktaState) (prefix : Key) (opts : QueryOptions) : Async<Choice<KVPairs * QueryMeta, Error>> =
  let getResponse = getResponse state "Fakta.KV.list"
  let req =
    UriBuilder.ofKVKey state.config prefix
    |> flip UriBuilder.mappendRange [ yield! opts.toKvs ()
                                      yield "recurse", None ]
    |> UriBuilder.uri
    |> createRequest Get
    |> acceptJson
    |> withIntroductions
    |> withQueryOpts state.config opts

  async {
    let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      let! body = Response.readBodyAsString resp
      let items = if body = "" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  }

/// Put is used to write a new value. Only the Key, Flags and Value is respected.
let put (state : FaktaState) (kvp : KVPair) (mCas : Index option) (opts : WriteOptions)
        : Async<Choice<WriteMeta, Error>> =
  let getResponse = getResponse state "Fakta.KV.put"
  match kvp.value with
  | None ->
    Logger.log state.logger (LogLine.sprintf ([ "value", box kvp])
               "Nulls? C'mon, use another library that for.")
    async.Return (Choice2Of2 (Message "see the logs"))
  | Some kvpValue ->
    let req =
      UriBuilder.ofKVKey state.config kvp.key
      |> fun ub ->
        [ if kvp.hasFlags then yield "flags", Some (kvp.flags.ToString())
          if Option.isSome mCas then yield "cas", Some ((Option.get mCas).ToString())
        ]
        |> UriBuilder.mappendRange ub
      |> UriBuilder.uri
      |> createRequest Put
      |> acceptJson
      |> withWriteOpts state.config opts
      |> withBodyValueField kvpValue

    async {
      let! response, dur = Duration.timeAsync (fun () -> getResponse req)
      use response = response
      match response.StatusCode with
      | 200 ->
        let! body = Response.readBodyAsString response
        match body with
        | "true" -> return Choice1Of2 { requestTime = dur }
        | "false" -> return Choice2Of2 CASFailed
        | x -> return Choice2Of2 (Message x)
      | x -> return Choice2Of2 (Message (sprintf "unkown status code %d for response %A" x response))
    }

/// Release is used for a lock release operation. The Key, Flags, Value and Session are respected. Returns true on success or false on failures. 
let release (s : FaktaState) (pair : KVPair) (wo : WriteOptions) : Async<Choice<bool * WriteMeta, Error>> =
  raise (TBD "TODO")