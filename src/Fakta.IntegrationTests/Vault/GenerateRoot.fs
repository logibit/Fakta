module Fakta.IntegrationTests.GenerateRoot

open Hopac
open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault
open System

let generateRootInitTest =
  let otp = 
    Array.init 16 (fun i -> byte(i*i))
    |> Convert.ToBase64String

  let listing = GenerateRoot.init vaultState (("otp", otp), [])
  ensureSuccess listing <| fun resp ->
    let logger = state.logger
    logger.logSimple (Message.sprintf Debug "New root generation result: %A" resp)
    resp.nonce

let nonce = memo generateRootInitTest

[<Tests>]
let tests =
  testList "Vault root generation tests" [
    testCaseAsync "sys.generate-root.status -> get root generation status" <| async {
      let listing = GenerateRoot.status vaultState []
      do! ensureSuccess listing <| fun status ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Root generation status: %A" status)
    }

    testCase "sys.generate-root.init -> initializes a new root generation attempt" <| fun _ ->
      do ignore nonce

    testCaseAsync "sys.generate-root.cancel -> cancels any in-progress root generation attempt." <| async {
      let listing = GenerateRoot.cancel vaultState []
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Root generations cancelled.")
    }

//    testCase "sys.generate-root.update -> Update a master key" <| fun _ ->
//      let listing = GenerateRoot.Update initState ((initState.config.keys.Value.[0],nonce), [])
//      ensureSuccess listing <| fun resp ->
//        let logger = state.logger
//        logger.logSimple (Message.sprintf Debug "New root generation result: %A" resp)
]