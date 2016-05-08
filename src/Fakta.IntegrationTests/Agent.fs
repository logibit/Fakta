module Fakta.IntegrationTests.Agent

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
  let checkId = "testCheckRegistration"
  let serviceId = "testServiceRegistration"

  testList "Agent tests" [
    testCase "agent.services -> locally registered services" <| fun _ ->
      let listing = Agent.services state
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.logSimple (Message.sprintf [] "value: %s" l.Value.id)

    testCase "agent.members -> the known gossip members" <| fun _ ->
      let listing = Agent.members state (false, [])
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for l in listing do
          logger.logSimple (Message.sprintf [] "value: %s" l.name)

    testCase "agent.checks -> locally registered checks" <| fun _ ->
      let listing = Agent.checks state []
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for KeyValue (key, value) in listing do
          logger.logSimple (Message.sprintf [] "key: %s value: %s" key value.name)

    testCase "agent.self -> information about agent we are speaking to" <| fun _ ->
      let listing = Agent.self state []
      ensureSuccess listing <| fun (selfConfig, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "selfConfig: %A" selfConfig)

    testCase "agent.nodeName -> node name of the agent" <| fun _ ->
      let listing = Agent.nodeName state []
      ensureSuccess listing <| fun (name, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" name)
    
    testCase "agent.checkregister -> register a new check with the local agent" <| fun _ ->
      let listing = Agent.checkRegister state (AgentCheckRegistration.ttlCheck checkId, [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.logSimple (Message.sprintf [] "key: %O" (listing))

    testCase "can agent set pass ttl" <| fun _ ->
      let listing = Agent.passTTL state checkId "optional parameter - passing"
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "ttl updated: %s" "passing")

    testCase "can agent set warn ttl" <| fun _ ->
      let listing = Agent.warnTTL state checkId "optional parameter - warning"
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "ttl updated: %s" "warning")

    testCase "can agent set fail ttl" <| fun _ ->
      let listing = Agent.failTTL state checkId "optional parameter - failing"
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "ttl updated: %s" "failing")

    testCase "agent.deregister check" <| fun _ ->
      let listing = Agent.checkDeregister state (checkId, [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.logSimple (Message.sprintf [] "key: %s" "can service deregister service")

    testCase "agent.register service -> register a new service with the local agent" <| fun _ ->
      let listing = Agent.serviceRegister state (AgentServiceRegistration.serviceRegistration serviceId)
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.logSimple (Message.sprintf [] "key: %s" "can service register service")

    testCase "agent.deregister service" <| fun _ ->
      let listing = Agent.serviceDeregister state serviceId
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.logSimple (Message.sprintf [] "key: %s" "can service deregister service")

    testCase "agent.join -> attempt a join to another cluster member" <| fun _ ->
      let listing = Agent.join state (("localhost", false), [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.logSimple (Message.sprintf [] "key: %s" "can service deregister service")

    testCase "can agent set node maintenance true" <| fun _ ->
      let listing = Agent.enableNodeMaintenance state ("enable maintenance", [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" "empty")

    testCase "can agent set node maintenance false" <| fun _ ->
      let listing = Agent.disableNodeMaintenance state ("disable maintenance", [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" "empty")

    testCase "can agent set service maintenance true" <| fun _ ->
      let listing = Agent.enableServiceMaintenance state (("consul", "for testing"), [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" "empty")

    testCase "can agent set service maintenance false" <| fun _ ->
      let listing = Agent.disableServiceMaintenance state (("consul", "not testing anymore"), [])
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "key: %s" "empty")

    testCase "agent.force.leave -> the agent ejects a failed node" <| fun _ ->
      let node = "COMP05"
      let listing = Agent.forceLeave state node
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "Node left: %s" node)
  ]