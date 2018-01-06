module Fakta.Tests.KV

open Expecto
open Expecto.Flip
open Fakta

[<Tests>]
let tests =
  testList "basics" [
    for expected, actual, name in
      [ "/v1/kv/a", Impl.keyFor "kv" "a", "a"
        "/v1/kv/", Impl.keyFor "kv" "", "empty string"
        "/v1/kv/", Impl.keyFor "kv" "/", "slash"
        "/v1/kv/a/b", Impl.keyFor "kv" "a/b", "hiera, no trailing slash"
        "/v1/kv/a/b/", Impl.keyFor "kv" "a/b/", "hiera, trailing slash"
      ] ->
      testCase (sprintf "format key value — %s" name) <| fun _ ->
        actual |> Expect.equal "Should format path" expected
  ]