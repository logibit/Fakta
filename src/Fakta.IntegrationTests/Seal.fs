module Fakta.IntegrationTests.Seal

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
  testList "Vault un/seal tests" [
    testCase "sys.sealStatus -> get application seal status" <| fun _ ->
      let listing = Seal.SealStatus vaultState []
      ensureSuccess listing <| fun (status) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Application is sealed: %A" status.``sealed``)

    testCase "sys.seal -> seals the vault" <| fun _ ->
      let listing = Seal.Seal vaultState []
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Vault sealed.")

    testCase "sys.unseal -> unseals the vault" <| fun _ ->
      let listing = Seal.Unseal initState (initState.config.keys.Value.[0], [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Vault sealed.")

    testCase "sys.resetUnsealProces" <| fun _ ->
      let listing = Seal.Unseal initState (initState.config.keys.Value.[0], [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Vault sealed.")
]