module Fakta.Event

open Fakta
open Fakta.Impl
open System
open HttpFs.Client
open Chiron
open Hopac

let faktaEventString = "Fakta.event"

let eventDottedPath (funcName: string) =
  [| "Fakta"; "Event"; funcName |]

// no warnings for lesser generalisation
#nowarn "64"

let internal eventPath (operation: string) =
  [| "Fakta"; "Event"; operation |]

let internal writeFilters state singlePath =
  writeFilters state (eventPath singlePath)
  >> respBodyFilter
  >> writeMetaFilter

let internal queryFilters state =
  eventPath >> queryFilters state

/// Fire is used to fire a new user event. Only the Name, Payload and Filters are respected. This returns the Id or an associated error. Cross DC requests are supported.
/// Returns the id of the event back.
let fire state : WriteCall<UserEvent, string> =

  let createRequest (event, opts) =
    let nodeVal, nodeKey = if event.nodeFilter.Equals("") then "","" else "node", event.nodeFilter
    let serviceVal, serviceKey = if event.nodeFilter.Equals("") then "","" else "service", event.serviceFilter
    let tagVal, tagKey = if event.nodeFilter.Equals("") then "","" else "tag", event.tagFilter

    UriBuilder.ofEvent state.config (sprintf "fire/%s" event.name)
    |> UriBuilder.mappendRange [ yield nodeVal, Some nodeKey
                                 yield serviceVal, Some serviceKey
                                 yield tagVal, Some tagKey ]
    |> UriBuilder.toUri
    |> basicRequest state.config Put

  let filters =
    writeFilters state "fire"
    >> codec createRequest (fstOfJsonPrism ConsulResult.objectId)

  HttpFs.Client.getResponse |> filters

/// IDToIndex is a bit of a hack. This simulates the index generation to convert an event ID into a WaitIndex.
let idToIndex (state: FaktaState) (uuid: Guid) : uint64 =
  let lower = uuid.ToString().Substring(0, 18).Replace("-","")
  let upper = uuid.ToString().Substring(19, 17).Replace("-","")
  let lowVal  = UInt64.Parse(lower, System.Globalization.NumberStyles.HexNumber)
  let highVal = UInt64.Parse(upper, System.Globalization.NumberStyles.HexNumber)
  lowVal ^^^ highVal

/// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. 
/// This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader.
let list state: QueryCall<(*name*) string, UserEvent list> =
  let createRequest (name, opts) =
    queryCall state.config "event/list" opts
    |> Request.queryStringItem "name" name

  let filters =
    queryFilters state "list"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters