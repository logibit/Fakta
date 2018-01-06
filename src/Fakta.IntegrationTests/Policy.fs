module Fakta.IntegrationTests.Policy

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault policies tests" [
    testCaseAsync "sys.policiesList -> lists all the available policies. " <| async {
      let! initState = initState
      let listing = Policy.policiesList initState []
      do! ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Policies list: %A" p)
    }

    testCaseAsync "sys.policiesList -> retrieve the rules for the named policy. " <| async {
      let! initState = initState
      let listing = Policy.getPolicy initState ("default", [])
      do! ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Policy's rules: %A" p)
    }

    testCaseAsync "sys.policyPut -> Add or update a policy. " <| async {
      let! initState = initState
      let rulesMap = Map.empty.Add("rules", """path "secret/*" {policy = "write"}""")
      let listing = Policy.putPolicy initState ((rulesMap, "secret"), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Policy added")
    }

    testCaseAsync "sys.policyDelete -> Delete the policy with the given name. " <| async {
      let! initState = initState
      let listing = Policy.deletePolicy initState ("secret", [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Policy deleted")
    }
  ]