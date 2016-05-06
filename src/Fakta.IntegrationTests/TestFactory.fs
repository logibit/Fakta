[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory 

open System
open NodaTime
open Fakta
open Fakta.Logging
open Fuchu

let config = FaktaConfig.Default

let logger =
  { new Logger with
      member x.Verbose f_line = printfn "[V] %A" (f_line ())
      member x.Debug f_line   = printfn "[V] %A" (f_line ())
      member x.Log line       = printfn "[V] %A" line }

let state = { config = config
              logger = logger //NoopLogger //logger
              clock  = SystemClock.Instance 
              random = Random () }

let ensureSuccess value f =
  match Async.RunSynchronously value with
  | Choice1Of2 x -> f x
  | Choice2Of2 err -> Tests.failtestf "got error %A" err

let given value = ensureSuccess value ignore