module Fakta.Event

open System
open System.Text
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open NodaTime
open HttpFs.Client
open Chiron

/// Fire is used to fire a new user event. Only the Name, Payload and Filters are respected. This returns the ID or an associated error. Cross DC requests are supported.
let fire (state : FaktaState) (event : UserEvent) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Event.fire"
  let nodeVal, nodeKey = if event.nodeFilter.Equals("") then "","" else "node", event.nodeFilter
  let serviceVal, serviceKey = if event.nodeFilter.Equals("") then "","" else "service", event.serviceFilter
  let tagVal, tagKey = if event.nodeFilter.Equals("") then "","" else "tag", event.tagFilter

  let req =
    UriBuilder.ofEvent state.config (sprintf "fire/%s" event.name)
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config
    |> withQueryStringItem nodeVal nodeKey
    |> withQueryStringItem serviceVal serviceKey
    |> withQueryStringItem tagVal tagKey
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "event.fire")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  item = if body = "" then UserEvent.EmptyEvent else Json.deserialize (Json.parse body)
        return Choice1Of2 (item.id, writeMeta dur)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// IDToIndex is a bit of a hack. This simulates the index generation to convert an event ID into a WaitIndex.
let idToIndex (state : FaktaState) (uuid : Guid) : uint64 =
  let lower = uuid.ToString().Substring(0, 18).Replace("-","")
  let upper = uuid.ToString().Substring(19, 17).Replace("-","")
  let lowVal  = UInt64.Parse(lower, System.Globalization.NumberStyles.HexNumber)
  let highVal = UInt64.Parse(upper, System.Globalization.NumberStyles.HexNumber)
  lowVal ^^^ highVal

/// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader. 
let list (state : FaktaState) (name : string) (opts : QueryOptions) : Async<Choice<UserEvent list * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Event.list"

  let req =
    UriBuilder.ofEvent state.config "list"
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
      | 404 -> return Choice2Of2 (Message "event.list")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}