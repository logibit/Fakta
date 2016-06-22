module Fakta.IntegrationTests.HealthVault

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
  testList "Vault health tests" [
    testCase "sys.vaultHealth -> get health status of vault" <| fun _ ->
      let listing = Health.GetHealth initState []
      ensureSuccess listing <| fun (h) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Health state: %A" h)

        ]