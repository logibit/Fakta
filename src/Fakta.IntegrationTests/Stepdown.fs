module Fakta.IntegrationTests.Stepdown

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault stepdown tests" [
    testCase "sys.stepdown -> Forces the node to give up active status" <| fun _ ->
      let listing = Stepdown.Stepdown vaultState []
      ensureSuccess listing <| fun (resp) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Node stepped-down.")
]