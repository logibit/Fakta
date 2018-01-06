module Fakta.IntegrationTests.Auth

open Expecto
open Fakta
open Fakta.Logging
open Fakta.Vault


[<Tests>]
let tests =
  testList "Vault auth tests" [
    testCaseAsync "sys.enableAuth -> enable a new auth backend" <| async {
      let! initState = initState
      let map = Map.empty.Add("type", AuthMethod.AppID.ToString())
      let listing = Auth.authEnable initState ((map, AuthMethod.AppID.ToString()), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "New auth backend enabled")
    }

    testCaseAsync "sys.authList -> lists all the enabled auth backends" <| async {
      let! initState = initState
      let listing = Auth.authList initState []
      do! ensureSuccess listing <| fun h ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Auth list: %A" h)
    }

    testCaseAsync "sys.disableAuth -> disable a new auth backend" <| async {
      let! initState = initState
      let listing = Auth.authDisable initState (AuthMethod.AppID.ToString(), [])
      do! ensureSuccess listing <| fun _ ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "Auth backend disabled")
    }
  ]