module Fakta.IntegrationTests.Status

open Expecto
open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  testList "Status tests" [
    testCase "status.leader -> query for a known leader" <| fun _ ->
      let listing = Status.leader state []
      ensureSuccess listing <| fun (leader) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %s" leader)

    testCase "status.peers -> query for a known raft peers " <| fun _ ->
      let listing = Status.peers state []
      ensureSuccess listing <| fun peers ->
        let logger = state.logger
        for peer in peers do
          logger.logSimple (Message.sprintf Debug "value: %s" peer)
]
