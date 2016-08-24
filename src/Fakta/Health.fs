module Fakta.Health
open Fakta
open Fakta.Impl
open HttpFs.Client

let healthDottedPath (funcName: string) =
  [| "Fakta"; "Health"; funcName |]

#nowarn "64"

let internal healthPath (operation: string) =
  [| "Fakta"; "Health"; operation |]

let internal writeFilters state =
  healthPath >> writeFilters state

let internal queryFilters state =
  healthPath >> queryFilters state

let getValuesByName state (path : string) (action:string): QueryCall<string, HealthCheck list> =  
  let createRequest =
    fun (name, opts) ->string name, opts
    >> queryCallEntityUri state.config path
    >> basicRequest state.config Get

  let filters =
    queryFilters state action
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Checks is used to return the checks associated with a service
let checks (state : FaktaState) : QueryCall<string, HealthCheck list> =
  let checks =
    fun (n, wo) ->
      getValuesByName state ("health/checks/")  "checks" (n, wo)
  checks


/// Node is used to query for checks belonging to a given node
let node (state : FaktaState) : QueryCall<string, HealthCheck list> =
  let node =
    fun (n, wo) ->
      getValuesByName state ("health/node/")  "node" (n, wo)
  node

/// State is used to retrieve all the checks in a given state.
/// The wildcard "any" state can also be used for all checks.
let state (state : FaktaState): QueryCall<string, HealthCheck list> =
  let state = 
    fun (n, wo) ->
      getValuesByName state ("health/state/")  "state" (n, wo)
  state

/// Service is used to query health information along with service info for a given service. It can optionally 
/// do server-side filtering on a tag or nodes with passing health checks only.
let service state: QueryCall<string * (*tag*) string * (*passingOnly*) bool, ServiceEntry list> =
  let createRequest ((serviceName, tag, passingOnly), opts) =
    queryCall state.config ("health/service/"+serviceName) opts
    |> Request.queryStringItem "tag" tag
    |> Request.queryStringItem "passing" (string passingOnly |> String.toLowerInvariant)

  let filters =
    queryFilters state "service"
    >> codec createRequest fstOfJson

  getResponse |> filters