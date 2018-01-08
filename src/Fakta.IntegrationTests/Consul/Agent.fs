module Fakta.IntegrationTests.Agent

open Hopac
open Expecto
open System
open Fakta
open Fakta.Logging
open Fakta.Logging.Message

[<Tests>]
let tests =
  let localService =
    async {
      let serviceId = Guid.NewGuid().ToString()
      let serviceName = Guid.NewGuid().ToString()
      let op = AgentServiceRegistration.serviceRegistration serviceId serviceName
      let res = Agent.serviceRegister state (op, [])
      do! ensureSuccessB res printResult
      return serviceId, serviceName
    }

  let localCheck =
    async {
      let! sid, _ = localService
      let checkId = Guid.NewGuid().ToString()
      let checkName = Guid.NewGuid().ToString()
      let regcmd = AgentCheckRegistration.ttlCheck checkId checkName sid "15s"
      let listing = Agent.checkRegister state (regcmd, [])
      do! ensureSuccessB listing printResult
      return checkId
    }

  let serviceId = "testServiceRegistration"

  testList "Agent" [
    testCaseAsync "services -> locally registered services" <| async {
      let listing = Agent.services state []
      do! ensureSuccessB listing <| fun (services, meta) -> job {
        for s in services do
          do! logger.debugWithBP (eventX "Service Id: {serviceId}" >> setField "serviceId" s.Value.id)
        do! logger.debugWithBP (eventX "Service meta: {meta}" >> setField "meta" meta)
      }
    }

    testCaseAsync "members -> the known gossip members" <| async {
      let listing = Agent.members state (false, [])
      do! ensureSuccessB listing <| fun (listing, meta) -> job {
        for l in listing do
          do! logger.debugWithBP (eventX "Member {name}" >> setField "name" l.name)
      }
    }

    testCaseAsync "checks -> locally registered checks" <| async {
      let listing = Agent.checks state []
      do! ensureSuccessB listing <| fun (listing, meta) -> job {
        for KeyValue (key, value) in listing do
          do! logger.debugWithBP (eventX "key: {key}, value: {value}" >> setField "key" key >> setField "value" value.name)
      }
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

    testList "checks" [
      testCaseAsync "checkRegister" <| async {
        let! _ = localCheck
        ()
      }

      testCaseAsync "set PASS TTL" <| async {
        let! checkId = localCheck
        let listing = Agent.passTTL state ((checkId, "optional parameter - passing"),[])
        do! ensureSuccess listing <| fun () ->
          logger.logSimple (Message.sprintf Debug "ttl updated: %s" "passing")
      }

      testCaseAsync "set WARN TTL" <| async {
        let! checkId = localCheck
        let listing = Agent.warnTTL state ((checkId, "optional parameter - warning"),[])
        do! ensureSuccess listing <| fun listing ->
          logger.logSimple (Message.sprintf Debug "ttl updated: %s" "warning")
      }

      testCaseAsync "set FAIL TTL" <| async {
        let! checkId = localCheck
        let listing = Agent.failTTL state ((checkId, "optional parameter - failing"),[])
        do! ensureSuccess listing <| fun listing ->
          logger.logSimple (Message.sprintf Debug "ttl updated: %s" "failing")
      }

      testCaseAsync "deregister check" <| async {
        let! checkId = localCheck
        let listing = Agent.checkDeregister state (checkId, [])
        do! ensureSuccess listing <| fun listing ->
          logger.logSimple (Message.sprintf Debug "key: %s" "can service deregister service")
      }
    ]

    testList "services" [
      testCaseAsync "register" <| async {
        let! _ = localService
        ()
      }

      testCaseAsync "deregister" <| async {
        let! sid, _ = localService
        let op = Agent.serviceDeregister state (sid, [])
        do! ensureSuccessB op printResult
      }
    ]

    testCaseAsync "join -> attempt a join to another cluster member" <| async {
      let listing = Agent.join state (("localhost", false), [])
      do! ensureSuccess listing <| fun (listing) ->
        logger.logSimple (Message.sprintf Debug "key: %s" "can service deregister service")
    }

    testCaseAsync "set node maintenance true" <| async {
      let op = Agent.enableNodeMaintenance state ("enable maintenance", [])
      do! ensureSuccessB op printResult
    }

    testCaseAsync "set node maintenance false" <| async {
      let op = Agent.disableNodeMaintenance state ("disable maintenance", [])
      do! ensureSuccessB op printResult
    }

    testCaseAsync "set service maintenance true" <| async {
      let! sid, _ = localService
      let enable = Agent.enableServiceMaintenance state ((sid, "for testing"), [])
      do! ensureSuccessB enable printResult
    }

    testCaseAsync "set service maintenance false" <| async {
      let! sid, _ = localService
      let enable = Agent.disableServiceMaintenance state ((sid, "for testing"), [])
      do! ensureSuccessB enable printResult
    }

    testCaseAsync "force.leave -> the agent ejects a failed node" <| async {
      let node = "COMP05"
      let op = Agent.forceLeave state (node, [])
      do! ensureSuccessB op printResult
    }
  ]