module Fakta.IntegrationTests.ACL

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging


[<Tests>]
let tests =  
  let tokenID = 
    let listing = ACL.create state (ACLEntry.ClientTokenInstance "" "client token" "client") []
    ensureSuccess listing <| fun (createdId, meta) ->
      let logger = state.logger
      logger.Log (LogLine.sprintf [] "acl id: %s" createdId)
      logger.Log (LogLine.sprintf [] "value: %A" meta)
      createdId

  testList "ACL tests" [
    testCase "ACL.list -> all the ACL tokens " <| fun _ ->
      let listing = ACL.list state []
      ensureSuccess listing <| fun (aclNodes, meta) ->
        let logger = state.logger
        for acl in aclNodes do
          logger.Log (LogLine.sprintf [] "acl id: %s" acl.id)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "ACL create" <| fun _ ->
      tokenID |> ignore

    testCase "ACL clone" <| fun _ ->
      let listing = ACL.clone state tokenID []
      ensureSuccess listing <| fun (clonedId, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "acl id: %s" clonedId)
        logger.Log (LogLine.sprintf [] "value: %A" meta)
        

    testCase "ACL info" <| fun _ ->
      let listing = ACL.info state tokenID []
      ensureSuccess listing <| fun (info, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "value: %s" info.id)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "ACL rules update" <| fun _ ->
      let listing = ACL.update state (ACLEntry.ClientTokenInstance tokenID "client token" "client") []
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "value: %A" meta)    

    testCase "ACL destroy" <| fun _ ->
      let listing = ACL.destroy state tokenID []
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "value: %A" meta)
]

