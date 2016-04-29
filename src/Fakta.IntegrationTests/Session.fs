module Fakta.IntegrationTests.Session

open System
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  let ttl = TTL (Duration.FromSeconds 10L)
  let sessionId =
    let listing = Session.create state [ttl] []
    ensureSuccess listing <| fun (sessionID, meta) ->
      let logger = state.logger       
      logger.Log (LogLine.sprintf [] "create session with id: %s" sessionID)
      sessionID

  testList "session tests" [
    testCase "create session" <| fun _ ->
      sessionId |> ignore 

    testCase "get session info" <| fun _ ->
      let listing = Session.info state sessionId []
      ensureSuccess listing <| fun (entry, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "session id: %O name: %s" entry.id entry.name)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "renew session" <| fun _ ->
      let listing = Session.renew state sessionId []
      ensureSuccess listing <| fun (entry, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "session id: %O name: %s" entry.id entry.name)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

//    testCase "renew session periodically" <| fun _ ->
//      Async.RunSynchronously (Session.renewPeriodic state (Duration.FromSeconds 10L) sessionId [] (Duration.FromSeconds 30L))

    testCase "get list of sessions" <| fun _ ->
      let listing = Session.list state []
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for entry in listing do
          logger.Log (LogLine.sprintf [] "session id: %O name: %s" entry.id entry.name)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "get list of sessions for a node" <| fun _ ->
      let listing = Session.node state "COMP05" []
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for entry in listing do
          logger.Log (LogLine.sprintf [] "session id: %O name: %s" entry.id entry.name)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "create sessions with no heathchecks" <| fun _ ->
      let listing = Session.createNoChecks state [ttl] []
      ensureSuccess listing <| fun (entry, meta) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "session id: %s" entry)
        logger.Log (LogLine.sprintf [] "value: %A" meta)

    testCase "destroy session" <| fun _ ->
      let listing = Session.destroy state sessionId []
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "destroyed: %O" listing)
  ]