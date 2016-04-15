module Fakta.IntegrationTests.Catalog

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
  testList "Catalog tests" [
    testCase "can catalog services" <| fun _ ->
      let listing = Catalog.datacenters state
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "value: %s" l)

    testCase "can catalog nodes" <| fun _ ->
      let listing = Catalog.nodes state []
      ensureSuccess listing <| fun (nodes, meta) ->
        let logger = state.logger
        for node in nodes do
          logger.Log (LogLine.sprintf [] "key: %s" node.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)
    

    testCase "can catalog node" <| fun _ ->
      let listing = Catalog.node state "COMP05" []
      ensureSuccess listing <| fun (node, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "key: %s" node.node.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can catalog service" <| fun _ ->
      let listing = Catalog.service state "consul" "" []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.Log (LogLine.sprintf [] "service node: %s" service.node)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can catalog services" <| fun _ ->
      let listing = Catalog.services state []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.Log (LogLine.sprintf [] "service key: %s  value length: %i" service.Key service.Value.Length)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can catalog register" <| fun _ ->
      let listing = Catalog.register state CatalogRegistration.Instance []
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can catalog deregister" <| fun _ ->
      let listing = Catalog.deregister state CatalogDeregistration.Instance []
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "meta: %A" meta)
  ]

