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

/// Datacenters is used to query for all the known datacenters
let datacenters (state : FaktaState) : Async<Choice<string list, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Catalog.datacenters"
  let req =
    UriBuilder.ofCatalog state.config "datacenters" 
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
      | 404 -> return Choice2Of2 (Message "agent.Checks")
      | _ ->
        let! body = Response.readBodyAsString resp
        let  item = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (item)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// 
let deregister (state : FaktaState) (dereg : CatalogDeregistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used")

/// Node is used to query for service information about a single node
let node (state : FaktaState) (node : string) (opts : QueryOptions) : Async<Choice<CatalogNode * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Catalog.node"
  let req =
    UriBuilder.ofCatalog state.config (sprintf "node/%s" node)
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
      | 404 -> return Choice2Of2 (Message "agent.nodes")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then CatalogNode.emptyCatalogNode else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Nodes is used to query all the known nodes
let nodes (state : FaktaState) (opts : QueryOptions) : Async<Choice<Node list * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Catalog.nodes"
  let req =
    UriBuilder.ofCatalog state.config "nodes" 
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
      | 404 -> return Choice2Of2 (Message "agent.nodes")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

///
let register (state : FaktaState) (reg : CatalogRegistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used")

/// Service is used to query catalog entries for a given service
let service (state : FaktaState) (service : string) (tag : string) (opts : QueryOptions) : Async<Choice<CatalogService list * QueryMeta, Error>> =
  raise (TBD "not used")

/// Service is used to query catalog entries for a given service
let services (state : FaktaState) (opts : QueryOptions) : Async<Choice<Map<string, string list> * QueryMeta, Error>> =
  raise (TBD "not used")
  