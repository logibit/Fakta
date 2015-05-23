module Fakta.IntegrationTests.KV

open System
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  testList "KV interactions" [
    testCase "can list" <| fun _ ->
      let listing = KV.list state "/" []
      ensureSuccess listing <| fun (kvpairs, meta) ->
        let logger = state.logger
        for kvp in kvpairs do
          logger.Log (LogLine.sprintf [] "key: %s, value: %A" kvp.key kvp.value)
        logger.Log (LogLine.sprintf [] "meta: %A" meta)

    testCase "can put" <| fun _ ->
      let pair = KVPair.Create("world", "goodbye")
      let put = KV.put state pair None []
      ensureSuccess put ignore

    testCase "can put -> get" <| fun _ ->
      given (KV.put state (KVPair.Create("monkey", "business")) None [])
      ensureSuccess (KV.get state "monkey" []) <| fun (kvp, _) ->
        Assert.Equal("monkeys do monkey business", "business", kvp.utf8String)
  ]