module Fakta.IntegrationTests.Status

open Expecto
open Fakta
open Hopac
open Hopac

[<Tests>]
let tests =
  testList "Status" [
    testCaseAsync "leader -> query for a known leader" <| async {
      let listing = Status.leader state []
      do! ensureSuccessB listing printResult
    }

    testCaseAsync "peers -> query for a known raft peers " <| async {
      let listing = Status.peers state []
      do! ensureSuccessB listing (List.map printResult >> Job.conIgnore)
    }
  ]