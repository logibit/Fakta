module Fakta.IntegrationTests.KV

open System.Net
open Chiron
open Hopac
open Expecto
open Expecto.Flip
open Fakta
open Fakta.Logging

type EPInfo =
  { ep : IPEndPoint }
  static member ToJson (epi: EPInfo) =
    Json.write "endpoint" (epi.ep.ToString())

let session =
  memo (ensureSuccess (Session.create state ([SessionOption.Name "kv-interactions-test"], [])) id)

[<Tests>]
let tests =
  testList "KV interactions" [
    testCaseAsync "can put" <| async {
      let pair = KVPair.Create("world", "goodbye")
      let put = KV.put state ((pair, None), [])
      do! ensureSuccess put ignore
    }

    testCaseAsync "can put -> get" <| async {
      do! given (KV.put state ((KVPair.Create("monkey", "business"), None), []))
      do! ensureSuccess (KV.get state ("monkey", [])) <| fun (kvp, _) ->
        kvp.utf8String |> Expect.equal "monkeys do monkey business" "business"
    }

    testCaseAsync "can list" <| async {
      let listing = KV.list state ("/", [])
      do! ensureSuccess listing <| fun (kvpairs, meta) ->
        let logger = state.logger
        for kvp in kvpairs do
          logger.logSimple (Message.sprintf Debug "key: %s, value: %A" kvp.key kvp.value)
        logger.logSimple (Message.sprintf Debug "meta: %A" meta)
    }

    testCaseAsync "can acquire -> release" <| async {
      let epInfo = { ep = IPEndPoint(IPAddress.IPv6Loopback, 8083) }
      let! session = session
      let kvp = KVPair.CreateForAcquire(session, "service/foo-router/mutex/send-email", epInfo, 1337UL)

      let acquire =
        job {
          let! res, _ = ensureSuccess (KV.acquire state (kvp, [])) id
          if not res then failtest "Failed to acquire lock"
        }

      let release =
        job {
          let! res, _ = ensureSuccess (KV.release state (kvp, [])) id
          if not res then failtest "failed to release lock"
        }

      let destroy =
        ensureSuccess (Session.destroy state (session, [])) ignore

      do! Job.tryFinallyJob (Job.tryFinallyJob acquire release) destroy
    }
  ]