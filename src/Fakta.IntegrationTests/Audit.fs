module Fakta.IntegrationTests.Audit

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

let filePath = @"./audit.log"
let path = "vault-audit"

[<Tests>]
let tests =
  testList "Vault audit tests" [
    testCaseAsync "sys.enableAudit -> enable file audit" <| async {
      let audit: Audit =
        { path = Some path
          ``type`` = "file"
          description = "audit file"
          options = Some (Map [ "path", filePath ]) }

      let! initState = initState
      let listing = Audit.enableAudit initState (audit, [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit enabled")
    }

    testCaseAsync "sys.listAudit -> list of audits" <| async {
      let! initState = initState
      let listing = Audit.auditList initState []
      do! ensureSuccess listing <| fun audit ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" audit)
    }

    testCaseAsync "sys.hashAudit -> hash audit" <| async {
      let! initState = initState
      let input = Map.empty.Add("input", "testInputAbc")
      let listing = Audit.hashAudit initState ((path, input), [])
      do! ensureSuccess listing <| fun hash ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" hash)
    }

    testCaseAsync "sys.disableAudit -> disable audit" <| async {
      let! initState = initState
      let listing = Audit.disableAudit initState (path, [])
      do! ensureSuccess listing <| fun audit ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Audit list: %A" audit)
    }
 ]