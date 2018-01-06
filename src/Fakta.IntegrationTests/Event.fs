module Fakta.IntegrationTests.Event

open Expecto
open Fakta
open Fakta.Logging
open System

[<Tests>]
let tests =
  testList "Event tests" [
    testCaseAsync "can event fire" <| async {
      let listing = Event.fire state ((UserEvent.Instance "b54fe110-7af5-cafc-d1fb-afc8ba432b1c" "test event"), [])
      do! ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf Debug "value: %s" listing)
        logger.logSimple (Message.sprintf Debug "value: %O" (meta.requestTime))
    }

    testCaseAsync "events list" <| async {
      let listing = Event.list state ("", [])
      do ignore listing
      do! ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for l in listing do
          logger.logSimple (Message.sprintf Debug "event name: %s" l.name)
        logger.logSimple (Message.sprintf Debug "value: %O" (meta.requestTime))
    }

    testCaseAsync "can convert idToIndex" <| async {
      let listing = Event.idToIndex state (new Guid("b54fe110-7af5-cafc-d1fb-afc8ba432b1c"))
      do ignore listing
      logger.logSimple (Message.sprintf Debug "value: %O" (listing))
    }
]

