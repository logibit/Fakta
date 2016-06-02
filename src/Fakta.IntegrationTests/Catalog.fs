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
    testCase "catalog.datacenters -> all the known datacenters" <| fun _ ->
      let listing = Catalog.datacenters state []
      listing |> ignore      
      ensureSuccess listing <| fun (dcs, meta) ->
        let logger = state.logger
        for l in dcs do
          logger.logSimple (Message.sprintf [] "value: %s" l)

    testCase "catalog.nodes -> all the known nodes" <| fun _ ->
      let listing = Catalog.nodes state []
      ensureSuccess listing <| fun (nodes, meta) ->
        let logger = state.logger
        for node in nodes do
          logger.logSimple (Message.sprintf [] "key: %s" node.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)
    

    testCase "catalog.node -> service information about a single node" <| fun _ ->
      let listing = Catalog.node state ("COMP09", [])
      ensureSuccess listing <| fun (node, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" node.node.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "catalog.service -> entries for a given service" <| fun _ ->
      let listing = Catalog.service state (("consul", ""), [])
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf [] "service node: %s" service.node)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "catalog.services -> all known services " <| fun _ ->
      let listing = Catalog.services state []
      ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf [] "service key: %s  value length: %i" service.Key service.Value.Length)
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "catalog.register" <| fun _ ->
       
      let agentCheck:AgentCheck = 
        { node = "COMP05"
          checkID = "service:consulcheck"
          name = "consul test health check"
          status = "passing"
          notes = ""
          output = ""
          serviceId = "consul"
          serviceName = "consul" }

      let agentService:AgentService = 
        { id = "consul"
          service = "consul"
          tags = Some([])
          port = Port.MinValue
          address = "127.0.0.1"
          enableTagOverride = false
          createIndex = 12
          modifyIndex = 18 }

      let catalogReg = (CatalogRegistration.Instance "COMP05" "127.0.0.1" "" agentCheck agentService)
      let listing = Catalog.register state (catalogReg, [])
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "meta: %A" meta)

    testCase "catalog.deregister" <| fun _ ->
      let catalogDereg = (CatalogDeregistration.Instance "COMP05" "dc1" "127.0.0.1" "consul" "")
      let listing = Catalog.deregister state (catalogDereg, [])
      ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "meta: %A" meta)
  ]

