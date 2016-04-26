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
let leader (state : FaktaState) : Async<Choice<string, Error>> =  
    let urlPath = "leader"
    let uriBuilder = UriBuilder.ofStatus state.config urlPath
    let result = Async.RunSynchronously (call state (statusDottedPath urlPath) id uriBuilder HttpMethod.Get)
    async {
      match result with 
      | Choice1Of2 x -> 
          let body, (dur:Duration, resp:Response) = x
          let item = if body = "" then "" else Json.deserialize (Json.parse body)
          return Choice1Of2 (item)
      | Choice2Of2 err -> return Choice2Of2(err)
    }

/// Peers is used to query for a known raft peers 
let peers (state : FaktaState) : Async<Choice<string list, Error>> =
    let urlPath = "peers"
    let uriBuilder = UriBuilder.ofStatus state.config urlPath
    let result = Async.RunSynchronously (call state (statusDottedPath urlPath) id uriBuilder HttpMethod.Get)
    async {
      match result with 
      | Choice1Of2 x -> 
          let body, (dur:Duration, resp:Response) = x
          let items = if body = "" then [] else Json.deserialize (Json.parse body)
          return Choice1Of2 (items)
      | Choice2Of2 err -> return Choice2Of2(err)
    }