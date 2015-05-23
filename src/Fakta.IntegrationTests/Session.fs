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
      let sessionId, writeMeta = ensureSuccess (Session.create state [] []) id
      ensureSuccess (Session.destroy state sessionId []) <| fun writeMeta -> ()
  ]