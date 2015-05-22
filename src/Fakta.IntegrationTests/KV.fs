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
        member x.Verbose f_line = printfn "[V] %A" (f_line ())
        member x.Debug f_line   = printfn "[V] %A" (f_line ())
        member x.Log line       = printfn "[V] %A" line }
  let state = { config = config
                logger = NoopLogger //logger
                clock  = SystemClock.Instance 
                random = Random () }
  let wo = WriteOptions.ofConfig config

  let ensureSuccess value f =
    match Async.RunSynchronously value with
    | Choice1Of2 x -> f x
    | Choice2Of2 err -> Tests.failtestf "got error %A" err

  let given value = ensureSuccess value ignore

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
      let put = KV.put state pair None wo
      ensureSuccess put ignore

    testCase "can put -> get" <| fun _ ->
      given (KV.put state (KVPair.Create("monkey", "business")) None wo)
      ensureSuccess (KV.get state "monkey" []) <| fun (kvp, _) ->
        Assert.Equal("monkeys do monkey business", "business", kvp.utf8String)
  ]