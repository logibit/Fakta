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
        logger.Log (LogLine.sprintf [] "value: %s" listing)
        logger.Log (LogLine.sprintf [] "value: %O" (meta.requestTime))

    testCase "events list" <| fun _ ->
      let listing = Event.list state "" []
      listing |> ignore      
      ensureSuccess listing <| fun (listing, meta) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "event name: %s" l.name)
        logger.Log (LogLine.sprintf [] "value: %O" (meta.requestTime))
    
    testCase "can convert idToIndex" <| fun _ ->
      let listing = Event.idToIndex state (new Guid("b54fe110-7af5-cafc-d1fb-afc8ba432b1c"))
      listing |> ignore  
      logger.Log (LogLine.sprintf [] "value: %O" (listing))
        
]

