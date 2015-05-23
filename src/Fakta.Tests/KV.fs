module Fakta.Tests.KV

open Fuchu

open Fakta

[<Tests>]
let tests =
  testList "basics" [
    testCase "format key value" <| fun _ ->
      Assert.Equal("should be", "/v1/kv/a", Impl.keyFor "kv" "a")
      Assert.Equal("should be", "/v1/kv/", Impl.keyFor "kv" "")
      Assert.Equal("should be", "/v1/kv/", Impl.keyFor "kv" "/")
      Assert.Equal("should be", "/v1/kv/a/b", Impl.keyFor "kv" "a/b")
      Assert.Equal("should be", "/v1/kv/a/b/", Impl.keyFor "kv" "a/b/")
  ]