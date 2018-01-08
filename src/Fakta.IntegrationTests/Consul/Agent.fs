module Fakta.IntegrationTests.Agent

open Expecto
open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  let checkId = "testCheckRegistration"
  let serviceId = "testServiceRegistration"

  testList "Agent" [
    testCaseAsync "services -> locally registered services" <| async {
      let listing = Agent.services state []
      do! ensureSuccess listing <| fun (services, meta) ->
        for l in services do
          logger.logSimple (Message.sprintf Debug "value: %s" l.Value.id)
    }

    testCaseAsync "members -> the known gossip members" <| async {
      let listing = Agent.members state (false, [])
      do! ensureSuccess listing <| fun (listing, meta) ->
        for l in listing do
          logger.logSimple (Message.sprintf Debug "value: %s" l.name)
    }

    testCaseAsync "checks -> locally registered checks" <| async {
      let listing = Agent.checks state []
      do! ensureSuccess listing <| fun (listing, meta) ->
        for KeyValue (key, value) in listing do
          logger.logSimple (Message.sprintf Debug "key: %s value: %s" key value.name)
    }

    testCaseAsync "self -> information about agent we are speaking to" <| async {
      let listing = Agent.self state []
      do! ensureSuccess listing <| fun (selfConfig, meta) ->
        logger.logSimple (Message.sprintf Debug "selfConfig: %A" selfConfig)
    }

    testCaseAsync "nodeName -> node name of the agent" <| async {
      let listing = Agent.nodeName state []
      do! ensureSuccess listing <| fun (name, meta) ->
        logger.logSimple (Message.sprintf Debug "key: %s" name)
    }

    testCaseAsync "checkregister -> register a new check with the local agent" <| async {
      let listing = Agent.checkRegister state ((AgentCheckRegistration.ttlCheck checkId "web app" "consul" "30s" "15s"), [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %O" (listing))
    }

    testCaseAsync "set PASS TTL" <| async {
      let listing = Agent.passTTL state  ((checkId, "optional parameter - passing"),[])
      do! ensureSuccess listing <| fun () ->
        logger.logSimple (Message.sprintf Debug "ttl updated: %s" "passing")
    }

    testCaseAsync "set WARN TTL" <| async {
      let listing = Agent.warnTTL state ((checkId, "optional parameter - warning"),[])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "ttl updated: %s" "warning")
    }

    testCaseAsync "set FAIL TTL" <| async {
      let listing = Agent.failTTL state ((checkId, "optional parameter - failing"),[])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "ttl updated: %s" "failing")
    }

    testCaseAsync "deregister check" <| async {
      let listing = Agent.checkDeregister state (checkId, [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "can service deregister service")
    }

    testCaseAsync "register service -> register a new service with the local agent" <| async {
      let listing = Agent.serviceRegister state ((AgentServiceRegistration.serviceRegistration serviceId),[])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "can service register service")
    }

    testCaseAsync "deregister service" <| async {
      let listing = Agent.serviceDeregister state (serviceId,[])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "can service deregister service")
    }

    testCaseAsync "join -> attempt a join to another cluster member" <| async {
      let listing = Agent.join state (("localhost", false), [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "can service deregister service")
    }

    testCaseAsync "set node maintenance true" <| async {
      let listing = Agent.enableNodeMaintenance state ("enable maintenance", [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "empty")
    }

    testCaseAsync "set node maintenance false" <| async {
      let listing = Agent.disableNodeMaintenance state ("disable maintenance", [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "empty")
    }

    testCaseAsync "set service maintenance true" <| async {
      let listing = Agent.enableServiceMaintenance state (("consul", "for testing"), [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "empty")
    }

    testCaseAsync "set service maintenance false" <| async {
      let listing = Agent.disableServiceMaintenance state (("consul", "not testing anymore"), [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "empty")
    }

    testCaseAsync "force.leave -> the agent ejects a failed node" <| async {
      let node = "COMP05"
      let listing = Agent.forceLeave state (node, [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "Node left: %s" node)
    }
  ]