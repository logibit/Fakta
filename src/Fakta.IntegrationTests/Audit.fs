module Fakta.IntegrationTests.Audit

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault

let filePath = @"./audit.log"
let path = "vault-audit"

[<Tests>]
let tests =
  testList "Vault audit tests" [
    testCase "sys.enableAudit -> enable file audit" <| fun _ ->
      let audit: Audit = 
        { path = Some(path)
          ``type`` = "file"
          description = "audit file"
          options = Some(Map.empty.Add("path", filePath))}

      let listing = Audit.enableAudit initState (audit, [])
      ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit enabled")

    testCase "sys.listAudit -> list of audits" <| fun _ ->
      let listing = Audit.auditList initState []
      ensureSuccess listing <| fun audit ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" audit)

    testCase "sys.hashAudit -> hash audit" <| fun _ ->
      let input = Map.empty.Add("input", "testInputAbc")
      let listing = Audit.hashAudit initState ((path, input), [])
      ensureSuccess listing <| fun hash ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" hash)
    
    testCase "sys.disableAudit -> disable audit" <| fun _ ->
      let listing = Audit.disableAudit initState (path, [])
      ensureSuccess listing <| fun audit ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" audit)


 ]
