module Fakta.Health
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac

let healthDottedPath (funcName: string) =
  [| "Fakta"; "Health"; funcName |]

#nowarn "64"

let healthPath (operation: string) =
  [| "Fakta"; "Health"; operation |]

let writeFilters state =
  healthPath >> writeFilters state

let queryFilters state =
  healthPath >> queryFilters state

let getValuesByName state (action:string) (path : string): QueryCall<string, HealthCheck list> =
  let createRequest =
    queryCallEntityUri state.config action
    >> basicRequest state.config Get

  let filters =
    queryFilters state path
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

//let getValuesByName (action : string) (path : string[]) (state : FaktaState) (service : string) (opts : QueryOptions)
//  : Job<Choice<HealthCheck list * QueryMeta, Error>> = job {
//  let urlPath = action
//  let uriBuilder = UriBuilder.ofHealth state.config urlPath
//                    |> UriBuilder.mappendRange (queryOptsKvs opts)
//  let! result = call state path id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

/// Checks is used to return the checks associated with a service
let checks (state : FaktaState) (service : string) (opts : QueryOptions) : QueryCall<string, HealthCheck list> =
  getValuesByName state ("health/checks/" + service)  "checks"


/// Node is used to query for checks belonging to a given node
let node (state : FaktaState) (node : string) (opts : QueryOptions) : QueryCall<string, HealthCheck list> =
  getValuesByName state ("health/node/" + node)  "node"

let state (state : FaktaState) (endpointState : string) (opts : QueryOptions) : QueryCall<string, HealthCheck list> =
  getValuesByName state ("health/state/" + endpointState)  "state"

let service state: QueryCall<(string * (*tag*) string * (*passingOnly*) bool), ServiceEntry list> =
  let createRequest ((serviceName, tag, passingOnly), opts) =
    queryCall state.config "health/service" opts
    |> Request.queryStringItem "tag" tag
    |> Request.queryStringItem "passing" (string passingOnly |> String.toLowerInvariant)

  let filters =
    queryFilters state "service"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
//let service (state : FaktaState) (service : string) (tag : string)
//  (passingOnly : bool) (opts : QueryOptions)
//  : Job<Choice<ServiceEntry list * QueryMeta, Error>> = job {
//  let urlPath = ("service/" + service)
//  let uriBuilder =
//    UriBuilder.ofHealth state.config ("service/" + service)
//      |> UriBuilder.mappendRange
//        [ if not (tag.Equals("")) then yield "tag", Some(tag)
//          if passingOnly then yield "passing", None
//          yield! queryOptsKvs opts]
//  let! result = call state (healthDottedPath "service") id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}
