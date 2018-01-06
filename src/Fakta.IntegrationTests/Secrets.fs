module Fakta.IntegrationTests.Secrets

open System
open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

let genericSecretPath = "secret"
let consulSecretPath = "consul"
let pkiPath = "pki"

[<Tests>]
let testsGeneric =
  testList "Vault generic secrets tests" [
    testCaseAsync "sys.secret.write -> create a new secret" <| async {
      let! initState = initState
      let data = Map.empty.Add("foo", "bar")
      let listing = Secrets.write initState ((data, genericSecretPath+"/secretOne"), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Secret created.")
    }

    testCaseAsync "sys.secret.read-> read a secret" <| async {
      let! initState = initState
      let listing = Secrets.read initState (genericSecretPath+"/secretOne", [])
      do! ensureSuccess listing <| fun sc ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Secret read: %A" sc)
    }

    testCaseAsync "sys.secret.list-> get a list of secret's names" <| async {
      let! initState = initState
      let listing = Secrets.list initState (genericSecretPath, [])
      do! ensureSuccess listing <| fun sc ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Secret list: %A" sc)
    }

    testCaseAsync "sys.secret.delete-> delete a secret" <| async {
      let! initState = initState
      let listing = Secrets.delete initState (genericSecretPath+"/secretOne", [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Secret deleted.")
    }
  ]

open System.Text

[<Tests>]
let testsConsul =
  testList "Vault consul secrets tests" [
    testCaseAsync "consul.config.access -> configure vault to know how to contact consul" <| async {
      let! initState = initState
      let! token = ACL.tokenId
      let config =
        Map [
          "token", token
          "address", state.config.serverBaseUri.ToString()
        ]
      let listing = Secrets.write initState ((config, consulSecretPath+"/config/access"), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Consul config sent to vault.")
    }

    testCaseAsync "consul.roles -> create a new role" <| async {
      let! initState = initState
      let policy64 =
        "read"
        |> UTF8Encoding.UTF8.GetBytes
        |> Convert.ToBase64String
      let config = Map.empty.Add("token_type", "management")
      let listing = Secrets.write initState ((config, consulSecretPath+"/roles/management"), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Consul role sent to vault.")
    }

    testCaseAsync "consul.roles -> queries a consul role definiton" <| async {
      let! initState = initState
      let listing = Secrets.read initState (consulSecretPath+"/roles/management", [])
      do! ensureSuccess listing <| fun sc ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Consul role read: %A" sc)
    }
  ]

//[<Tests>]
//let testsPki =
//  testList "Vault pki cert secrets tests" [
//    testCaseAsync "pki.root.generate.internal -> generate root certificate" <| async {
//      let! initState = initState
//      let config = Map.empty.Add("common_name", "myvault.com").Add("ttl", "87600h")
//      let listing = Secrets.WriteWithReturnValue initState ((config, pkiPath+"/root/generate/internal"), [])
//      do! ensureSuccess listing <| fun sc ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "Certigicate generated with return value: %A" sc)
//    }
//  ]