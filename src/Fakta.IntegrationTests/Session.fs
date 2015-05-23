module Fakta.IntegrationTests.Session

open System
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  testList "session tests" [
    testCase "create and destroy session" <| fun _ ->
      let ttl = TTL (Duration.FromSeconds 10L)
      //let state = { state with config = { state.config with serverBaseUri = Uri "http://127.0.0.1:8501" } }
      let sessionId, writeMeta = ensureSuccess (Session.create state [ttl] []) id
      ensureSuccess (Session.destroy state sessionId []) <| fun writeMeta -> ()
  ]