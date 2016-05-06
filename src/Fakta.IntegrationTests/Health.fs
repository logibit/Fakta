module Fakta.IntegrationTests.Health

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging


[<Tests>]
let tests =
  testList "Health tests" [
    testCase "health.node -> checks of given node" <| fun _ ->
      let listing = Health.node state "comp05" []
      ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.logSimple (Message.sprintf [] "key: %s, value: %s" check.name check.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "health.checks -> checks associated with a service" <| fun _ ->
      let listing = Health.checks state "serviceReg" []
      ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.logSimple (Message.sprintf [] "key: %s, value: %s" check.name check.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "health.service -> health and service information" <| fun _ ->
      let listing = Health.service state "consul" "" true []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf [] "key: %s, value: %s" service.service.service service.node.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "health.state -> checks in a given state" <| fun _ ->
      let listing = Health.state state "any" []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf [] "key: %s, value: %s" service.name service.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)
  ]

