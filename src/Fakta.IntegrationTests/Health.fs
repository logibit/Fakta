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
    testCase "can health node" <| fun _ ->
      let listing = Health.node state "comp05" []
      ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.Log (LogLine.sprintf [] "key: %s, value: %s" check.name check.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can health checks" <| fun _ ->
      let listing = Health.checks state "serviceReg" []
      ensureSuccess listing <| fun (checks, meta) ->
        let logger = state.logger
        for check in checks do
          logger.Log (LogLine.sprintf [] "key: %s, value: %s" check.name check.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can health service" <| fun _ ->
      let listing = Health.service state "consul" "" true []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.Log (LogLine.sprintf [] "key: %s, value: %s" service.service.service service.node.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can health state" <| fun _ ->
      let listing = Health.state state "any" []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.Log (LogLine.sprintf [] "key: %s, value: %s" service.name service.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)
  ]

