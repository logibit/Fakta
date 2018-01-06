module Fakta.IntegrationTests.Leader

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault


[<Tests>]
let tests =
  testList "Vault leader tests" [
    testCaseAsync "sys.leader -> returns the high availability status and current leader instance of Vault" <| async {
      let listing = Leader.leader vaultState []
      do! ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Leader address: %s Ha_enabled: %A is_self: %A" resp.leaderAddress resp.haEnabled resp.isSelf)
    }
]