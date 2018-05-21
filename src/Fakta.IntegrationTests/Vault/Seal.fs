module Fakta.IntegrationTests.Seal

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault un/seal tests" [
    testCaseAsync "sys.sealStatus -> get application seal status" <| async {
      let listing = Seal.sealStatus vaultState []
      do! ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Application is sealed: %A" status.``sealed``)
    }

    testCaseAsync "sys.seal -> seals the vault" <| async {
      let listing = Seal.seal vaultState []
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault sealed.")
    }

    testCaseAsync "sys.unseal -> unseals the vault" <| async {
      let! initState = initState
      let listing = Seal.unseal initState (initState.config.keys.Value.[0], [])
      do! ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault unsealed: %A" resp)
    }

    testCaseAsync "sys.resetUnsealProces" <| async {
      let! initState = initState
      let listing = Seal.unseal initState (initState.config.keys.Value.[0], [])
      do! ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Vault unsealed: %A" resp)
    }
  ]