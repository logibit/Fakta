module Fakta.Status
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron

/// Leader is used to query for a known leader 
let leader (state : FaktaState) : Async<Choice<string, Error>> =  
  let getResponse = Impl.getResponse state "Fakta.status.leader"
  let req =
    UriBuilder.ofStatus state.config "leader" 
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
      | 404 -> return Choice2Of2 (Message "Leader")
      | _ ->
        let! body = Response.readBodyAsString resp
        let item = if body = "" then "" else Json.deserialize (Json.parse body)
        return Choice1Of2 (item)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Peers is used to query for a known raft peers 
let peers (state : FaktaState) : Async<Choice<string list, Error>> =
  let getResponse = Impl.getResponse state "Fakta.status.peers"
  let req =
    UriBuilder.ofStatus state.config "peers" 
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
      | 404 -> return Choice2Of2 (Message "peers")
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}