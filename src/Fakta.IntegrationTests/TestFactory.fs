[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory 

open System
open NodaTime
open Fakta
open Fakta.Logging
open Fuchu
open Hopac

let config = FaktaConfig.empty

let logger =
  { new Logger with
      member x.log message =
        printfn "[V] %A" message
        Alt.always (Promise.Now.withValue ())

      member x.logVerbose evaluate =
        printfn "[V] %A" (evaluate ())
        Alt.always (Promise.Now.withValue ())

      member x.logSimple message =
        printfn "[V] %A" message
    }

let state =
  { config = config
    logger = logger //NoopLogger //logger
    clock  = SystemClock.Instance 
    random = Random () }

let ensureSuccess computation kontinue =
  match run computation with
  | Choice1Of2 x ->
    kontinue x

  | Choice2Of2 err ->
    Tests.failtestf "got error %A" err

let given value = ensureSuccess value ignore