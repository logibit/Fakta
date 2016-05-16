module Fakta.IntegrationTests.Event

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
  testList "Event tests" [
    testCase "can event fire" <| fun _ ->
      let listing = Event.fire state (UserEvent.Instance "b54fe110-7af5-cafc-d1fb-afc8ba432b1c" "test event") []
      listing |> ignore      
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        logger.logSimple (Message.sprintf [] "value: %s" listing)
        logger.logSimple (Message.sprintf [] "value: %O" (meta.requestTime))

    testCase "events list" <| fun _ ->
      let listing = Event.list state "" []
      listing |> ignore      
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for l in listing do
          logger.logSimple (Message.sprintf [] "event name: %s" l.name)
        logger.logSimple (Message.sprintf [] "value: %O" (meta.requestTime))
    
    testCase "can convert idToIndex" <| fun _ ->
      let listing = Event.idToIndex state (new Guid("b54fe110-7af5-cafc-d1fb-afc8ba432b1c"))
      listing |> ignore  
      logger.logSimple (Message.sprintf [] "value: %O" (listing))
        
]

