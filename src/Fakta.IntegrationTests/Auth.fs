module Fakta.IntegrationTests.Auth

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
  testList "Vault auth tests" [
    testCase "sys.enableAuth -> enable a new auth backend" <| fun _ ->
      let map = Map.empty.Add("type", AuthMethod.AppID.ToString())
      let listing = Auth.AuthEnable initState ((map, AuthMethod.AppID.ToString()), [])
      ensureSuccess listing <| fun (_) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "New auth backend enabled")

    testCase "sys.authList -> lists all the enabled auth backends" <| fun _ ->
      let listing = Auth.AuthList initState []
      ensureSuccess listing <| fun (h) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Auth list: %A" h)

    testCase "sys.disableAuth -> disable a new auth backend" <| fun _ ->
      let listing = Auth.AuthDisable initState (AuthMethod.AppID.ToString(), [])
      ensureSuccess listing <| fun (_) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Auth backend disabled")
        ]