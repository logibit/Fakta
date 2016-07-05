module Fakta.IntegrationTests.Capabilities

open Fuchu
open Fakta
open Fakta.Logging
open Fakta.Vault

[<Tests>]
let tests =
  testList "Vault capabilities tests" [
    testCase "sys.capabilities -> capabilities of the token on the given path." <| fun _ ->
      let map = Map.empty.Add("token", initState.config.token.Value).Add("path", "secret")
      let listing = Capabilities.capabilities initState (map, [])
      ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Capabilities: %A" p)

    testCase "sys.capabilities-self -> capabilities of client token on the given path." <| fun _ ->
      let map = Map.empty.Add("path", "secret")
      let listing = Capabilities.capabilitiesSelf initState (map, [])
      ensureSuccess listing <| fun p ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Capabilities-self: %A" p)
        
        ]