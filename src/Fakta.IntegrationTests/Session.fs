module Fakta.IntegrationTests.Session

open Expecto
open NodaTime

open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  let ttl = TTL (Duration.FromSeconds 10L)
  let sessionId =
    let listing = Session.create state ([ttl], [])
    ensureSuccess listing <| fun (sessionID) ->
      let logger = state.logger       
      logger.logSimple (Message.sprintf Debug "create session with id: %s" sessionID)
      sessionID

  testList "session tests" [
    testCase "create session" <| fun _ ->
      sessionId |> ignore 

    testCaseAsync "get session info" <| async {
      let! initState = initState
      let! sessionId = sessionId
      let listing = Session.info state (sessionId, [])
      do! ensureSuccess listing <| fun (entry, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "session id: %O name: %s" entry.id entry.name)
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "renew session" <| async {
      let! sessionId = sessionId
      let listing = Session.renew state (sessionId, [])
      do! ensureSuccess listing <| fun (entry) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "session id: %O name: %s" entry.id entry.name)
    }

//    testCase "renew session periodically" <| fun _ ->
//      Async.RunSynchronously (Session.renewPeriodic state (Duration.FromSeconds 10L) sessionId [] (Duration.FromSeconds 30L))

    testCaseAsync "get list of sessions" <| async {
      let listing = Session.list state []
      do! ensureSuccess listing <| fun (list, meta) ->
        let logger = state.logger
        for entry in list do
          logger.logSimple (Message.sprintf Debug "session id: %O name: %s" entry.id entry.name)
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "get list of sessions for a node" <| async {
      let listing = Session.node state ("COMP05", [])
      do! ensureSuccess listing <| fun (node, meta) ->
        let logger = state.logger
        for entry in node do
          logger.logSimple (Message.sprintf Debug "session id: %O name: %s" entry.id entry.name)
        logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "create sessions with no heathchecks" <| async {
      let listing = Session.createNoChecks state ([ttl], [])
      do! ensureSuccess listing <| fun (entry) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "session id: %s" entry)
        //logger.logSimple (Message.sprintf Debug "value: %A" meta)
    }

    testCaseAsync "destroy session" <| async {
      let! sessionId = sessionId
      let listing = Session.destroy state (sessionId, [])
      do! ensureSuccess listing <| fun (entry) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "destroyed: %O" entry)
    }
  ]