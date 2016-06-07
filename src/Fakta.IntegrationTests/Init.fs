module Fakta.IntegrationTests.Init

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Vault

let vaultState = FaktaState.empty APIType.Vault "" []

[<Tests>]
let tests =
  testList "Vault init tests" [
    testCase "sys.initStatus -> get application init status" <| fun _ ->
      let listing = Init.InitStatus vaultState []
      ensureSuccess listing <| fun (map) ->
        let logger = state.logger
        for KeyValue (key, value) in map do
          logger.logSimple (Message.sprintf [] "key: %s value: %A" key value)

    testCase "sys.init -> initialize application" <| fun _ ->
      let reqJson : InitRequest =
         {secretShares = 1
          secretThreshold =1
          pgpKeys = []}
      let listing = Init.Init vaultState (reqJson, [])
      ensureSuccess listing <| fun resp ->
        let logger = state.logger
        //for peer in resp do
        logger.logSimple (Message.sprintf [] "value: %s" resp.rootToken)
]