module Fakta.Status
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron

let faktaStatusString = "Fakta.status"

let statusDottedPath (funcName: string) =
  (sprintf "%s.%s" faktaStatusString funcName)

/// Leader is used to query for a known leader
let leader (state : FaktaState) : Async<Choice<string, Error>> = async {
  let urlPath = "leader"
  let uriBuilder = UriBuilder.ofStatus state.config urlPath
  let! result = call state (statusDottedPath urlPath) id uriBuilder HttpMethod.Get

  match result with
  | Choice1Of2 (body, (dur, resp)) ->
    match Json.tryParse body with
    | Choice1Of2 json ->
      match Json.tryDeserialize json with
      | Choice1Of2 item -> return Choice1Of2(item)
      | Choice2Of2 err -> return Choice2Of2(Message err)
    | Choice2Of2 err -> return Choice2Of2(Message err)
    //let item = if body = "" then "" else Json.deserialize (Json.parse body)
    //return Choice1Of2 (item)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Peers is used to query for a known raft peers
let peers (state : FaktaState) : Async<Choice<string list, Error>> =  async {
  let urlPath = "peers"
  let uriBuilder = UriBuilder.ofStatus state.config urlPath
  let! result = call state (statusDottedPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items)
  | Choice2Of2 err -> return Choice2Of2(err)
}