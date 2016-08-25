module Fakta.IntegrationTests.Init

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault


[<Tests>]
let tests =
  testList "Vault init tests" [
    testCase "sys.initStatus -> get application init status" <| fun _ ->
      let listing = Init.initStatus vaultState []
      ensureSuccess listing <| fun (map) ->
        let logger = state.logger
        for KeyValue (key, value) in map do
          logger.logSimple (Message.sprintf Debug "key: %s value: %A" key value)

    testCase "sys.init -> initialize application" <| fun _ ->
      let s = initVault
      match s.config.token with
      | None -> Tests.failtest "Vault init failed."
      | _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %s" s.config.token.Value)
]
