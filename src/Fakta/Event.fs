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

let faktaEventString = "Fakta.event"

let eventDottedPath (funcName: string) =
  (sprintf "%s.%s" faktaEventString funcName)

/// Fire is used to fire a new user event. Only the Name, Payload and Filters are respected. This returns the ID or an associated error. Cross DC requests are supported.
let fire (state : FaktaState) (event : UserEvent) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> = async {
  let nodeVal, nodeKey = if event.nodeFilter.Equals("") then "","" else "node", event.nodeFilter
  let serviceVal, serviceKey = if event.nodeFilter.Equals("") then "","" else "service", event.serviceFilter
  let tagVal, tagKey = if event.nodeFilter.Equals("") then "","" else "tag", event.tagFilter
  let urlPath = (sprintf "fire/%s" event.name)
  let uriBuilder = UriBuilder.ofEvent state.config urlPath 
                    |> flip UriBuilder.mappendRange [ yield nodeVal, Some(nodeKey) 
                                                      yield serviceVal, Some(serviceKey)
                                                      yield tagVal, Some(tagKey)]
  let! result = call state (eventDottedPath "fire") id uriBuilder HttpMethod.Put
  match result with 
  | Choice1Of2 (body, (dur, resp)) -> 
      let  item = if body = "" then UserEvent.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (item.id, writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// IDToIndex is a bit of a hack. This simulates the index generation to convert an event ID into a WaitIndex.
let idToIndex (state : FaktaState) (uuid : Guid) : uint64 =
  let lower = uuid.ToString().Substring(0, 18).Replace("-","")
  let upper = uuid.ToString().Substring(19, 17).Replace("-","")
  let lowVal  = UInt64.Parse(lower, System.Globalization.NumberStyles.HexNumber)
  let highVal = UInt64.Parse(upper, System.Globalization.NumberStyles.HexNumber)
  lowVal ^^^ highVal

/// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader. 
let list (state : FaktaState) (name : string) (opts : QueryOptions) : Async<Choice<UserEvent list * QueryMeta, Error>> = async {
  let urlPath = "list"
  let uriBuilder = UriBuilder.ofEvent state.config urlPath
  let! result = call state (eventDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) -> 
      let  items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}