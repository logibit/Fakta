module Fakta.IntegrationTests.Capabilities

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
  testList "Vault capabilities tests" [
    testCase "sys.capabilities -> capabilities of the token on the given path." <| fun _ ->
      let map = Map.empty.Add("token", initState.config.token.Value).Add("path", "secret")
      let listing = Capabilities.Capabilities initState (map, [])
      ensureSuccess listing <| fun (p) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Capabilities: %A" p)

    testCase "sys.capabilities-self -> capabilities of client token on the given path." <| fun _ ->
      let map = Map.empty.Add("path", "secret")
      let listing = Capabilities.CapabilitiesSelf initState (map, [])
      ensureSuccess listing <| fun (p) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Capabilities-self: %A" p)
        
        ]