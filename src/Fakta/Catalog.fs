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

let faktaCatalogString = "Fakta.catalog"

let catalogDottedPath (funcName: string) =
  (sprintf "%s.%s" faktaCatalogString funcName)

/// Datacenters is used to query for all the known datacenters
let datacenters (state : FaktaState) : Async<Choice<string list, Error>> = async {
  let urlPath = "datacenters"
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) -> 
      let  item = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Node is used to query for service information about a single node
let node (state : FaktaState) (node : string) (opts : QueryOptions) : Async<Choice<CatalogNode * QueryMeta, Error>> = async {
  let urlPath = (sprintf "node/%s" node)
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let! result = call state (catalogDottedPath "node") id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) ->
      if body = "" 
      then 
        return Choice2Of2 (Message (sprintf "Node %s not found" node))
      else
        let items:CatalogNode =  Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// Nodes is used to query all the known nodes
let nodes (state : FaktaState) (opts : QueryOptions) : Async<Choice<Node list * QueryMeta, Error>> = async {
  let urlPath = "nodes"
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) -> 
      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// 
let deregister (state : FaktaState) (dereg : CatalogDeregistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> = async {
  let urlPath = "deregister"
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let serializedCheckReg = Json.serialize dereg |> Json.format
  let! result = call state (catalogDottedPath urlPath) (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
  match result with 
  | Choice1Of2 (_, (dur, _)) -> 
      return Choice1Of2 (writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}


///
let register (state : FaktaState) (reg : CatalogRegistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> = async {
  let urlPath = "register"
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let serializedCheckReg = Json.serialize reg |> Json.format
  let! result = call state (catalogDottedPath urlPath) (withJsonBody serializedCheckReg) uriBuilder HttpMethod.Put
  match result with 
  | Choice1Of2 (_, (dur, _)) -> 
      return Choice1Of2 (writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Service is used to query catalog entries for a given service
let service (state : FaktaState) (service : string) (tag : string) (opts : QueryOptions) 
  : Async<Choice<CatalogService list * QueryMeta, Error>> = async {
  let urlPath = (sprintf "service/%s" service)
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let! result = call state (catalogDottedPath "service") id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) ->
      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Service is used to query catalog entries for a given service
let services (state : FaktaState) (opts : QueryOptions) : Async<Choice<Map<string, string list> * QueryMeta, Error>> = async {
  let urlPath = "services"
  let uriBuilder = UriBuilder.ofCatalog state.config urlPath
  let! result = call state (catalogDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with 
  | Choice1Of2 (body, (dur, resp)) -> 
      let items = if body = "" then Map.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}
  