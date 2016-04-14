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
  testList "Agent tests" [
    testCase "can agent services" <| fun _ ->
      let listing = Agent.services state
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "value: %s" l.Value.id)

    testCase "can agent members" <| fun _ ->
      let listing = Agent.members state false
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "value: %s" l.name)

    testCase "can agent checks" <| fun _ ->
      let listing = Agent.checks state
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "key: %s value: %s" l.Key l.Value.name)

    testCase "can agent self" <| fun _ ->
      let listing = Agent.self state
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        for l in listing do
          logger.Log (LogLine.sprintf [] "key: %s" l.Key)

    testCase "can agent register check" <| fun _ ->
      let listing = Agent.checkRegister state AgentCheckRegistration.ttlCheck
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.Log (LogLine.sprintf [] "key: %s" (listing.ToString()))
        

//    testCase "can agent set node maintenance true" <| fun _ ->
//      let listing = Agent.enableNodeMaintenance state ""
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger
//        logger.Log (LogLine.sprintf [] "key: %s" "empty")
//
//    testCase "can agent set node maintenance false" <| fun _ ->
//      let listing = Agent.disableNodeMaintenance state 
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger
//        logger.Log (LogLine.sprintf [] "key: %s" "empty")

//    testCase "can agent set node maintenance false" <| fun _ ->
//      let listing = Agent.enableServiceMaintenance state "consul" ""
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger
//        logger.Log (LogLine.sprintf [] "key: %s" "empty")

//    testCase "can agent set node maintenance false" <| fun _ ->
//      let listing = Agent.disableServiceMaintenance state "consul" 
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger
//        logger.Log (LogLine.sprintf [] "key: %s" "empty")
        ]        


