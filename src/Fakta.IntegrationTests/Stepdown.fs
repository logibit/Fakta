module Fakta.IntegrationTests.Stepdown

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault stepdown tests" [
    testCase "sys.stepdown -> Forces the node to give up active status" <| fun _ ->
      let listing = Stepdown.stepdown vaultState []
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Node stepped-down.")
]