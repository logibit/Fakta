module Fakta.IntegrationTests.ACL

open Hopac
open Hopac.Infixes
open Expecto
open Fakta
open Fakta.Logging
open Fakta.Logging.Message

let tokenId =
  let aclInstance = ACLEntry.ClientTokenInstance "" "test management token" "management"
  let listing = ACL.create state (aclInstance , [])
  memo (
    ensureSuccessB listing <| fun createdId ->
      logger.debugWithBP (eventX "Created acl id: {ACLId}" >> setField "ACLId" createdId)
      |> Async.map (fun () -> createdId)
      |> Job.fromAsync
  )

[<Tests>]
let tests =
  testList "ACL" [
    testCaseAsync "list -> all the ACL tokens " <| async {
      let listing = ACL.list state []
      do! ensureSuccessB listing <| fun (aclNodes, meta) -> job {
        for acl in aclNodes do
          do! logger.debugWithBP (eventX "ACL id: {ACLId}" >> setField "ACLId" acl.id)
        do! logger.debugWithBP (eventX "Meta: {meta}" >> setField "meta" meta)
      }
    }

    testCase "create" <| fun _ ->
      ignore "tested for each test during the setup"

    testCaseAsync "clone" <| async {
      let! tokenId = tokenId
      let listing = ACL.clone state (tokenId, [])
      do! ensureSuccessA listing <| fun clonedId ->
      logger.debugWithBP (eventX "Cloned ACL id: {ACLId}" >> setField "ACLId" clonedId)
    }

    testCaseAsync "info" <| async {
      let! tokenId = tokenId
      let listing = ACL.info state (tokenId, [])
      do! ensureSuccessA listing <| fun (info, meta) ->
      logger.debugWithBP (eventX "Info ACL id: {ACLId}" >> setField "ACLId" info.id)
      |> Async.bind (fun _ -> logger.debugWithBP (eventX "Meta: {meta}" >> setField "meta" meta))
    }

    testCaseAsync "rules update" <| async {
      let! tokenId = tokenId
      let aclInstance = ACLEntry.ClientTokenInstance tokenId "client token" "client"
      let listing = ACL.update state (aclInstance, [])
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "destroy" <| async {
      let! tokenId = tokenId
      let listing = ACL.destroy state (tokenId, [])
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }
]

