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

    testCase "can agent nodeName" <| fun _ ->
      let listing = Agent.nodeName state
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "key: %s" listing)

    testCase "can force leave" <| fun _ ->
      let node = "COMP05"
      let listing = Agent.forceLeave state node
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger
        logger.Log (LogLine.sprintf [] "Node left: %s" node)

//    testCase "can agent pass ttl" <| fun _ ->
//      let listing = Agent.passTTL state "consul" "optional parameter"
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger
//        logger.Log (LogLine.sprintf [] "ttl updated: %s" "passing")

//    testCase "can agent register check" <| fun _ ->
//      let listing = Agent.checkRegister state AgentCheckRegistration.ttlCheck
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger 
//        logger.Log (LogLine.sprintf [] "key: %s" (listing.ToString()))
    

    testCase "can agent register service" <| fun _ ->
      let listing = Agent.serviceRegister state AgentServiceRegistration.serviceRegistration
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.Log (LogLine.sprintf [] "key: %s" "can service register service")

//    testCase "can agent deregister check" <| fun _ ->
//      let listing = Agent.checkDeregister state "serfHealth"
//      listing |> ignore      
//      ensureSuccess listing <| fun (listing) ->
//        let logger = state.logger 
//        logger.Log (LogLine.sprintf [] "key: %s" "can service deregister service")

    testCase "can agent deregister service" <| fun _ ->
      let listing = Agent.serviceDeregister state "service:serviceReg123"
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.Log (LogLine.sprintf [] "key: %s" "can service deregister service")

    testCase "can agent join address" <| fun _ ->
      let listing = Agent.join state "localhost" false
      listing |> ignore      
      ensureSuccess listing <| fun (listing) ->
        let logger = state.logger 
        logger.Log (LogLine.sprintf [] "key: %s" "can service deregister service")
        

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


