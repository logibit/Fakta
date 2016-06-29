module Fakta.IntegrationTests.Secrets

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Vault

let genericSecretPath = "secret"
let consulSecretPath = "consul"

[<Tests>]
let testsGeneric =
  testList "Vault generic secrets tests" [
    testCase "sys.secret.write -> create a new secret" <| fun _ ->
      let data = Map.empty.Add("foo", "bar")
      let listing = Secrets.Write initState ((data, genericSecretPath+"/secretOne"), [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Secret created.")

    testCase "sys.secret.read-> read a secret" <| fun _ ->
      let listing = Secrets.ReadNonRenewable initState (genericSecretPath+"/secretOne", [])
      ensureSuccess listing <| fun sc ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Secret read: %A" sc)

    testCase "sys.secret.list-> get a list of secret's names" <| fun _ ->
      let listing = Secrets.List initState (genericSecretPath, [])
      ensureSuccess listing <| fun sc ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Secret list: %A" sc)

    testCase "sys.secret.delete-> delete a secret" <| fun _ ->
      let listing = Secrets.Delete initState (genericSecretPath+"/secretOne", [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Secret deleted.")
]

