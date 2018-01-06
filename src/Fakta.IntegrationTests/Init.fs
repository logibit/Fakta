module Fakta.IntegrationTests.Init

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault


[<Tests>]
let tests =
  testList "Vault init tests" [
    testCaseAsync "sys.initStatus -> get application init status" <| async {
      let listing = Init.initStatus vaultState []
      do! ensureSuccess listing <| fun (map) ->
        let logger = state.logger
        for KeyValue (key, value) in map do
          logger.logSimple (Message.sprintf Debug "key: %s value: %A" key value)
    }

    testCaseAsync "sys.init -> initialize application" <| async {
      let! s = initVault
      match s.config.token with
      | None ->
        failtest "Vault init failed."
      | _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %s" s.config.token.Value)
    }
  ]