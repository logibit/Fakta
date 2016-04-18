module Fakta.IntegrationTests.Status

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
  testList "Status tests" [
    testCase "can status leader" <| fun _ ->
      let listing = Status.leader state
      ensureSuccess listing <| fun (leader) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "value: %s" leader)

    testCase "can status peers" <| fun _ ->
      let listing = Status.peers state
      ensureSuccess listing <| fun (peers) ->
        let logger = state.logger
        for peer in peers do
          logger.Log (LogLine.sprintf [] "value: %s" peer)
]
