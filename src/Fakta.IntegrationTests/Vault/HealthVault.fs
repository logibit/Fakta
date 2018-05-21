module Fakta.IntegrationTests.HealthVault

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault health tests" [
    testCaseAsync "sys.vaultHealth -> get health status of vault" <| async {
      let! initState = initState
      let listing = Health.getHealth initState []
      do! ensureSuccess listing <| fun h ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Health state: %A" h)
    }
  ]