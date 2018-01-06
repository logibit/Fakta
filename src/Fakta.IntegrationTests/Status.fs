module Fakta.IntegrationTests.Status

open Expecto
open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  testList "Status tests" [
    testCaseAsync "status.leader -> query for a known leader" <| async {
      let listing = Status.leader state []
      do! ensureSuccess listing <| fun (leader) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %s" leader)
    }

    testCaseAsync "status.peers -> query for a known raft peers " <| async {
      let listing = Status.peers state []
      do! ensureSuccess listing <| fun peers ->
        let logger = state.logger
        for peer in peers do
          logger.logSimple (Message.sprintf Debug "value: %s" peer)
    }
  ]