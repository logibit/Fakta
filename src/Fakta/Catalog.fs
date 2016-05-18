module Fakta.Catalog
open System
open System.Text
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open NodaTime
open HttpFs.Client
open Chiron
open Hopac

let faktaCatalogString = "Fakta.catalog"

let catalogDottedPath (funcName: string) =
  [| "Fakta"; "Catalog"; funcName |]

// no warnings for lesser generalisation
#nowarn "64"

let catalogPath (operation: string) =
  [| "Fakta"; "Catalog"; operation |]

let writeFilters state =
  catalogPath >> writeFilters state

let queryFilters state =
  catalogPath >> queryFilters state

let datacenters state : QueryCall<string list> =
  let createRequest =
    queryCall state.config "catalog/datacenters"

  let filters =
    queryFilters state "datacenters"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Datacenters is used to query for all the known datacenters
//let datacenters (state : FaktaState) : Job<Choice<string list, Error>> = job {
//  let urlPath = "datacenters"
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let  item = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (item)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let node state : QueryCall<string, CatalogNode> =
  let createRequest =
    queryCallEntityUri state.config "catalog/node"
    >> basicRequest state.config Get

  let filters =
    queryFilters state "node"
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

/// Node is used to query for service information about a single node
//let node (state : FaktaState) (node : string) (opts : QueryOptions) : Job<Choice<CatalogNode * QueryMeta, Error>> = job {
//  let urlPath = (sprintf "node/%s" node)
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let! result = call state (catalogDottedPath "node") id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      if body = ""
//      then
//        return Choice2Of2 (Message (sprintf "Node %s not found" node))
//      else
//        let items:CatalogNode =  Json.deserialize (Json.parse body)
//        return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let nodes state : QueryCall<Node list> =
  let createRequest =
    queryCall state.config "catalog/nodes"

  let filters =
    queryFilters state "nodes"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Nodes is used to query all the known nodes
//let nodes (state : FaktaState) (opts : QueryOptions) : Job<Choice<Node list * QueryMeta, Error>> = job {
//  let urlPath = "nodes"
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}


/// endpoint for directly removing entries from the Catalog. Note: it is usually preferrable 
/// instead to use the agent endpoints for deregistration as they are simpler and perform anti-entropy.
let deregister state : WriteCall<CatalogDeregistration, unit> =
  let createRequest (dereg, opts) =
    writeCallUri state.config "catelog/deregister" opts
    |> basicRequest state.config Put
    |> withJsonBodyT dereg

  let filters =
    writeFilters state "deregister"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

///
//let deregister (state : FaktaState) (dereg : CatalogDeregistration) (opts : WriteOptions) : Job<Choice<WriteMeta, Error>> = job {
//  let urlPath = "deregister"
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let serializedCheckReg = Json.serialize dereg
//  let! result = call state (catalogDottedPath urlPath) (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (_, (dur, _)) ->
//      return Choice1Of2 (writeMeta dur)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

///mechanism for registering or updating entries in the catalog. Note: it is usually preferrable 
///instead to use the agent endpoints for registration as they are simpler and perform anti-entropy.
let register state : WriteCall<CatalogRegistration, unit> =
  let createRequest (reg, opts) =
    writeCallUri state.config "catelog/register" opts
    |> basicRequest state.config Put
    |> withJsonBodyT reg

  let filters =
    writeFilters state "register"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

///
//let register (state : FaktaState) (reg : CatalogRegistration) (opts : WriteOptions) : Job<Choice<WriteMeta, Error>> = job {
//  let urlPath = "register"
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let serializedCheckReg = Json.serialize reg
//  let! result = call state (catalogDottedPath urlPath) (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
//  match result with
//  | Choice1Of2 (_, (dur, _)) ->
//      return Choice1Of2 (writeMeta dur)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let service state : QueryCall<((*service*) string * (*tag*) string), CatalogService list> =
  let createRequest ((service, tag), opts) =
    queryCall state.config "catalog/service" opts
    |> Request.queryStringItem "near" tag
    
  let filters =
    queryFilters state "service"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Service is used to query catalog entries for a given service
//let service (state : FaktaState) (service : string) (tag : string) (opts : QueryOptions)
//  : Job<Choice<CatalogService list * QueryMeta, Error>> = job {
//  let urlPath = (sprintf "service/%s" service)
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let! result = call state (catalogDottedPath "service") id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}

let services state: QueryCall<Map<string, string list>> =
  let createRequest =
    queryCall state.config "catalog/services"

  let filters =
    queryFilters state "services"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// Service is used to query catalog entries for a given service
//let services (state : FaktaState) (opts : QueryOptions) : Job<Choice<Map<string, string list> * QueryMeta, Error>> = job {
//  let urlPath = "services"
//  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
//  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
//  match result with
//  | Choice1Of2 (body, (dur, resp)) ->
//      let items = if body = "" then Map.empty else Json.deserialize (Json.parse body)
//      return Choice1Of2 (items, queryMeta dur resp)
//  | Choice2Of2 err -> return Choice2Of2(err)
//}
