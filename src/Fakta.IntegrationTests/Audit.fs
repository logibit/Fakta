module Fakta.IntegrationTests.Audit

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime
open Fakta
open Fakta.Logging
open Fakta.Vault

let filePath = @"C:\audit.log"
let path = "vault-audit"

[<Tests>]
let tests =
  testList "Vault audit tests" [
    testCase "sys.enableAudit -> enable file audit" <| fun _ ->
      let audit: Audit =
        {
            Path = Some(path)
            Type = "file"
            Description = "audit file"
            Options = Some(Map.empty.Add("path", filePath))
        }
      let listing = Audit.EnableAudit initState (audit, [])
      ensureSuccess listing <| fun (_) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Audit enabled")

    testCase "sys.listAudit -> list of audits" <| fun _ ->
      let listing = Audit.AuditList initState []
      ensureSuccess listing <| fun (audit) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Audit list: %A" audit)

    testCase "sys.hashAudit -> hash audit" <| fun _ ->
      let input = Map.empty.Add("input", "testInputAbc")
      let listing = Audit.HashAudit initState ((path, input), [])
      ensureSuccess listing <| fun (hash) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Audit list: %A" hash)
    
    testCase "sys.disableAudit -> disable audit" <| fun _ ->
      let listing = Audit.DisableAudit initState (path, [])
      ensureSuccess listing <| fun (audit) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Audit list: %A" audit)


 ]