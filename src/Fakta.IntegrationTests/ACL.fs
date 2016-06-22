module Fakta.IntegrationTests.ACL

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging

let tokenId =
  let aclInstance = ACLEntry.ClientTokenInstance "" "test management token" "management"
  let listing = ACL.create state (aclInstance , [])
  ensureSuccess listing <| fun createdId ->
    let logger = state.logger
    logger.logSimple (Message.sprintf [] "created acl id: %O" createdId)
    createdId

[<Tests>]
let tests =
  testList "ACL tests" [
    testCase "ACL.list -> all the ACL tokens " <| fun _ ->
      let listing = ACL.list state []
      ensureSuccess listing <| fun (aclNodes, meta) ->
        let logger = state.logger
        for acl in aclNodes do
          logger.logSimple (Message.sprintf [] "acl id: %O" acl.id)
        logger.logSimple (Message.sprintf [] "value: %A" meta)

    testCase "ACL create" <| fun _ ->
      ignore "tested for each test during the setup"

    testCase "ACL clone" <| fun _ ->
      let listing = ACL.clone state (tokenId, [])
      ensureSuccess listing <| fun clonedId ->
        state.logger.logSimple (Message.sprintf [] "acl id: %O" clonedId)

    testCase "ACL info" <| fun _ ->
      let listing = ACL.info state (tokenId, [])
      ensureSuccess listing <| fun (info, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "value: %O" info.id)
        logger.logSimple (Message.sprintf [] "value: %A" meta)

    testCase "ACL rules update" <| fun _ ->
      let aclInstance = ACLEntry.ClientTokenInstance tokenId "client token" "client"
      let listing = ACL.update state (aclInstance, [])
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "value: %A" meta)

    testCase "ACL destroy" <| fun _ ->
      let listing = ACL.destroy state (tokenId, [])
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "value: %A" meta)
]

