module Fakta.IntegrationTests.ACL

open Hopac
open Expecto
open Fakta
open Fakta.Logging

let tokenId =
  let aclInstance = ACLEntry.ClientTokenInstance "" "test management token" "management"
  let listing = ACL.create state (aclInstance , [])
  memo (
    ensureSuccess listing <| fun createdId ->
      let logger = state.logger
      logger.logSimple (Message.sprintf Debug "created acl id: %O" createdId)
      createdId
  )

[<Tests>]
let tests =
  testList "ACL tests" [
    testCaseAsync "ACL.list -> all the ACL tokens " <| async {
      let listing = ACL.list state []
      do! ensureSuccess listing <| fun (aclNodes, meta) ->
        let logger = state.logger
        for acl in aclNodes do
          logger.logSimple (Message.sprintf Debug "acl id: %O" acl.id)
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCase "ACL create" <| fun _ ->
      ignore "tested for each test during the setup"

    testCaseAsync "ACL clone" <| async {
      let! tokenId = tokenId
      let listing = ACL.clone state (tokenId, [])
      do! ensureSuccess listing <| fun clonedId ->
        state.logger.logSimple (Message.sprintf Debug "acl id: %O" clonedId)
    }

    testCaseAsync "ACL info" <| async {
      let! tokenId = tokenId
      let listing = ACL.info state (tokenId, [])
      do! ensureSuccess listing <| fun (info, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "info.id: %O" info.id)
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "ACL rules update" <| async {
      let! tokenId = tokenId
      let aclInstance = ACLEntry.ClientTokenInstance tokenId "client token" "client"
      let listing = ACL.update state (aclInstance, [])
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "ACL destroy" <| async {
      let! tokenId = tokenId
      let listing = ACL.destroy state (tokenId, [])
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }
]

