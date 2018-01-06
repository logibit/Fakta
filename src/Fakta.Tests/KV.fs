module Fakta.Tests.KV

open Expecto
open Expecto.Flip
open Fakta

[<Tests>]
let tests =
  testList "basics" [
    for expected, actual in
      [ "/v1/kv/a", Impl.keyFor "kv" "a"
        "/v1/kv/", Impl.keyFor "kv" ""
        "/v1/kv/", Impl.keyFor "kv" "/"
        "/v1/kv/a/b", Impl.keyFor "kv" "a/b"
        "/v1/kv/a/b/", Impl.keyFor "kv" "a/b/"
      ] ->
      testCase (sprintf "format key value %s" expected) <| fun _ ->
        actual |> Expect.equal "Should format path" expected
  ]