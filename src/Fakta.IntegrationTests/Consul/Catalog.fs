module Fakta.IntegrationTests.Catalog

open Expecto
open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  testList "Catalog" [
    testCaseAsync "catalog.datacenters -> all the known datacenters" <| async {
      let listing = Catalog.datacenters state []
      do ignore listing
      do! ensureSuccess listing <| fun (dcs, meta) ->
        let logger = state.logger
        for l in dcs do
          logger.logSimple (Message.sprintf Debug "value: %s" l)
    }

    testCaseAsync "catalog.nodes -> all the known nodes" <| async {
      let listing = Catalog.nodes state []
      do! ensureSuccess listing <| fun (nodes, meta) ->
        let logger = state.logger
        for node in nodes do
          logger.logSimple (Message.sprintf Debug "key: %s" node.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "catalog.node -> service information about a single node" <| async {
      let listing = Catalog.node state (System.Environment.MachineName, [])
      do! ensureSuccess listing <| fun (node, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "key: %s" node.node.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "catalog.service -> entries for a given service" <| async {
      let listing = Catalog.service state (("consul", ""), [])
      do! ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf Debug "service node: %s" service.node)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "catalog.services -> all known services " <| async {
      let listing = Catalog.services state []
      do! ensureSuccess listing <| fun (services, meta) ->
        let logger = state.logger
        for service in services do
          logger.logSimple (Message.sprintf Debug "service key: %s  value length: %i" service.Key service.Value.Length)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "catalog.register" <| async {
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
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "catalog.deregister" <| async {
      let catalogDereg = (CatalogDeregistration.Instance "COMP05" "" "127.0.0.1" "consul" "")
      let listing = Catalog.deregister state (catalogDereg, [])
      do! ensureSuccess listing <| fun (meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }
  ]

