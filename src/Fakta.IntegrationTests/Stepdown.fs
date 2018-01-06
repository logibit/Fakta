module Fakta.IntegrationTests.Stepdown

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault stepdown tests" [
    testCaseAsync "sys.stepdown -> Forces the node to give up active status" <| async {
      let listing = Stepdown.stepdown vaultState []
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Node stepped-down.")
    }
]