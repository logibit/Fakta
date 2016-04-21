module Fakta.ACL
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron

/// Clone is used to return a new token cloned from an existing one
let clone (state : FaktaState) (id : Id) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.clone"
  let req =
    UriBuilder.ofAcl state.config (sprintf "clone/%s" id)
    |> UriBuilder.uri
    |> basicRequest Put
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
      | 404 -> return Choice2Of2 (Message "clone")
      | _ ->
        let! body = Response.readBodyAsString resp
        let item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
        return Choice1Of2 (item.["ID"], writeMeta dur)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Create is used to generate a new token with the given parameters
let create (state : FaktaState) (tokenToCreate : ACLEntry) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.create"
  let json = Json.serialize tokenToCreate |> Json.format
  let req =
    UriBuilder.ofAcl state.config "create" 
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config
    |> withJsonBody json
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "List")
      | _ ->
        let! body = Response.readBodyAsString resp
        let item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
        return Choice1Of2 (item.["ID"], writeMeta dur)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Destroy is used to destroy a given ACL token ID 
let destroy (state : FaktaState) (id : Id) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.destroy"
  let req =
    UriBuilder.ofAcl state.config (sprintf "destroy/%s" id)
    |> UriBuilder.uri
    |> basicRequest Put
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
      | 404 -> return Choice2Of2 (Message "List")
      | _ ->
        let! body = Response.readBodyAsString resp
        return Choice1Of2 (writeMeta dur)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Info is used to query for information about an ACL token 
let info (state : FaktaState) (id : Id) (opts : QueryOptions) : Async<Choice<ACLEntry * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.info"
  let req =
    UriBuilder.ofAcl state.config (sprintf "info/%s" id)
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
      | 404 -> return Choice2Of2 (Message "Info")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items.[0], queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// List is used to get all the ACL tokens 
let list (state : FaktaState) (opts : QueryOptions) : Async<Choice<ACLEntry list * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.list"
  let req =
    UriBuilder.ofAcl state.config "list" 
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
      | 404 -> return Choice2Of2 (Message "List")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Update is used to update the rules of an existing token
let update (state : FaktaState) (acl : ACLEntry) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.acl.update"
  let json = Json.serialize acl |> Json.format
  let req =
    UriBuilder.ofAcl state.config "update" 
    |> UriBuilder.uri
    |> basicRequest Put
    |> withConfigOpts state.config
    |> withJsonBody json
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message "List")
      | _ ->
        let! body = Response.readBodyAsString resp
        return Choice1Of2 (writeMeta dur)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}