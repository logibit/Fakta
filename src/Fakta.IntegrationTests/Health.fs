module Fakta.IntegrationTests.Health

open Expecto
open Fakta
open Fakta.Logging


[<Tests>]
let tests =
  testList "Health tests" [
    testCaseAsync "health.node -> checks of given node" <| async {
      let listing = Health.node state ("comp05", [])
      do! ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.logSimple (Message.sprintf Debug "key: %s, value: %s" check.name check.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "health.checks -> checks associated with a service" <| async {
      let listing = Health.checks state ("serviceReg", [])
      do! ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.logSimple (Message.sprintf Debug "key: %s, value: %s" check.name check.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "health.service -> health and service information" <| async {
      let listing = Health.service state (("consul", "", true), [])
      do! ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf Debug "key: %s, value: %s" service.service.service service.node.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "health.state -> checks in a given state" <| async {
      let listing = Health.state state ("any", [])
      do! ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf Debug "key: %s, value: %s" service.name service.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }
  ]