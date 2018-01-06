﻿module Fakta.IntegrationTests.HealthVault

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault


[<Tests>]
let tests =
  testList "Vault health tests" [
    testCase "sys.vaultHealth -> get health status of vault" <| fun _ ->
      let listing = Health.getHealth initState []
      ensureSuccess listing <| fun h ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Health state: %A" h)

        ]