module Fakta.Tests.KV

open Fuchu

open Fakta

[<Tests>]
let tests =
  testList "basics" [
    testCase "format key value" <| fun _ ->
      let subject = Impl.keyFor "a"
      Assert.Equal("should be /v1/kv/a", "/v1/kv/a", subject)
  ]

()