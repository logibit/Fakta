module Fakta.IntegrationTests.Seal

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault un/seal tests" [
    testCase "sys.sealStatus -> get application seal status" <| fun _ ->
      let listing = Seal.sealStatus vaultState []
      ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Application is sealed: %A" status.``sealed``)

    testCase "sys.seal -> seals the vault" <| fun _ ->
      let listing = Seal.seal vaultState []
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault sealed.")

    testCase "sys.unseal -> unseals the vault" <| fun _ ->
      let listing = Seal.unseal initState (initState.config.keys.Value.[0], [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault unsealed: %A" resp)

    testCase "sys.resetUnsealProces" <| fun _ ->
      let listing = Seal.unseal initState (initState.config.keys.Value.[0], [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault unsealed: %A" resp)
]