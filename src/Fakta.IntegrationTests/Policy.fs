module Fakta.IntegrationTests.Policy

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
  testList "Vault policies tests" [
    testCase "sys.policiesList -> lists all the available policies. " <| fun _ ->
      let listing = Policy.PoliciesList initState []
      ensureSuccess listing <| fun (p) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Policies list: %A" p)

    testCase "sys.policiesList -> retrieve the rules for the named policy. " <| fun _ ->
      let listing = Policy.GetPolicy initState ("default", [])
      ensureSuccess listing <| fun (p) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Policy's rules: %A" p)

    testCase "sys.policyPut -> Add or update a policy. " <| fun _ ->    
      let rulesMap = Map.empty.Add("rules", """path "secret/*" {policy = "write"}""")
      let listing = Policy.PutPolicy initState ((rulesMap, "secret"), [])
      ensureSuccess listing <| fun (_) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Policy added")

    testCase "sys.policyDelete -> Delete the policy with the given name. " <| fun _ ->    
      let listing = Policy.DeletePolicy initState ("secret", [])
      ensureSuccess listing <| fun (_) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Policy deleted")
        ]