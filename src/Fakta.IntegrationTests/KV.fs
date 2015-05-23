module Fakta.IntegrationTests.KV

open System
open System.Net
open Chiron
open Chiron.Operators
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging

type EPInfo =
  { ep : IPEndPoint }
  static member ToJson (epi : EPInfo) =
    Json.write "endpoint" (epi.ep.ToString())

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

    testCase "can acquire -> release" <| fun _ ->
      let epInfo = { ep = IPEndPoint(IPAddress.IPv6Loopback, 8083) }
      let session, _ = ensureSuccess (Session.create state [SessionOption.Name "kv-interactions-test"] []) id
      let kvp = KVPair.CreateForAcquire(session, "service/foo-router/mutex/send-email", epInfo, 1337UL)
      try
        try
          let res, _ = ensureSuccess (KV.acquire state kvp []) id
          if not res then Tests.failtest "failed to acquire lock"
        finally
          let res, _ = ensureSuccess (KV.release state kvp []) id
          if not res then Tests.failtest "failed to release lock"
      finally
        given (Session.destroy state session [])
        
  ] 