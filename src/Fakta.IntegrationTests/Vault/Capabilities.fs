module Fakta.IntegrationTests.Capabilities

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault capabilities tests" [
    testCaseAsync "sys.capabilities -> capabilities of the token on the given path." <| async {
      let! initState = initState
      let map = Map.empty.Add("token", initState.config.token.Value).Add("path", "secret")
      let listing = Capabilities.capabilities initState (map, [])
      do! ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Capabilities: %A" p)
    }

    testCaseAsync "sys.capabilities-self -> capabilities of client token on the given path." <| async {
      let! initState = initState
      let map = Map.empty.Add("path", "secret")
      let listing = Capabilities.capabilitiesSelf initState (map, [])
      do! ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Capabilities-self: %A" p)
    }
  ]