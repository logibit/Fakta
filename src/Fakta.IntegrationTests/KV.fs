module Fakta.IntegrationTests.KV

open System
open Fuchu
open NodaTime

open Fakta
open Fakta.Logging

[<Tests>]
let tests =
  let config = FaktaConfig.Default
  let logger =
    { new Logger with
        member x.Verbose f_line =
          printfn "[V] %A" (f_line ())
        member x.Debug f_line =
          printfn "[V] %A" (f_line ())
        member x.Log line =
          printfn "[V] %A" line }
  let state = { config = config
                logger = logger
                clock  = SystemClock.Instance 
                random = Random () }
  let qo = QueryOptions.ofConfig config
  let wo = WriteOptions.ofConfig config

  let ensureSuccess value f =
    match Async.RunSynchronously value with
    | Choice1Of2 x -> f x
    | Choice2Of2 err -> Tests.failtestf "got error %A" err

  testList "KV interactions" [
    testCase "can list" <| fun _ ->
      let listing = KV.list state "/" qo
      ensureSuccess listing <| fun (kvpairs, meta) -> ()

    testCase "can put" <| fun _ ->
      let pair = KVPair.Create("world", "goodbye")
      let put = KV.put state pair None wo
      ensureSuccess put <| fun _ -> ()
  ]