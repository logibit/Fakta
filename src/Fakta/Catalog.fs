module Fakta.Catalog

open Fakta
open Fakta.Impl
open HttpFs.Client

let faktaCatalogString = "Fakta.catalog"

let catalogDottedPath (funcName: string) =
  [| "Fakta"; "Catalog"; funcName |]

// no warnings for lesser generalisation
#nowarn "64"

let internal catalogPath (operation: string) =
  [| "Fakta"; "Catalog"; operation |]

let internal writeFilters state =
  catalogPath >> writeFilters state

let internal queryFilters state =
  catalogPath >> queryFilters state

/// Datacenters is used to query for all the known datacenters
let datacenters state : QueryCall<string list> =
  let createRequest =
    queryCall state.config "catalog/datacenters"

  let filters =
    queryFilters state "datacenters"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Node is used to query for service information about a single node
let node state : QueryCall<string, CatalogNode> =
    let createRequest =
      fun (name, opts) -> string name, opts
      >> queryCallEntityUri state.config "catalog/node" 
      >> basicRequest state.config Get

    let filters =
        queryFilters state "node"
        >> codec createRequest fstOfJson 

    HttpFs.Client.getResponse |> filters

/// Nodes is used to query all the known nodes
let nodes state : QueryCall<Node list> =
  let createRequest =
    queryCall state.config "catalog/nodes"

  let filters =
    queryFilters state "nodes"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters


/// endpoint for directly removing entries from the Catalog. Note: it is usually preferrable 
/// instead to use the agent endpoints for deregistration as they are simpler and perform anti-entropy.
let deregister state : WriteCallNoMeta<CatalogDeregistration, unit> =
  let createRequest (dereg, opts) =
    writeCallUri state.config "catalog/deregister" opts
    |> basicRequest state.config Put
    |> withJsonBodyT dereg

  let filters =
    writeFilters state "deregister"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

///mechanism for registering or updating entries in the catalog. Note: it is usually preferrable 
///instead to use the agent endpoints for registration as they are simpler and perform anti-entropy.
let register state : WriteCallNoMeta<CatalogRegistration, unit> =
  let createRequest (reg, opts) =
    writeCallUri state.config "catalog/register" opts
    |> basicRequest state.config Put
    |> withJsonBodyT reg

  let filters =
    writeFilters state "register"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

/// Service is used to query catalog entries for a given service
let service state : QueryCall<((*service*) string * (*tag*) string), CatalogService list> =
  let createRequest ((service, tag), opts) =
    queryCall state.config ("catalog/service/"+service) opts
    |> Request.queryStringItem "near" tag
    
  let filters =
    queryFilters state "service"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Service is used to query catalog entries for a given service
let services state: QueryCall<Map<string, string list>> =
  let createRequest =
    queryCall state.config "catalog/services"

  let filters =
    queryFilters state "services"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters