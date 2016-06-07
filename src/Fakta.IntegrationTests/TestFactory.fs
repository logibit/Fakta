[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory 

open System
open NodaTime
open Fakta
open Fakta.Logging
open Fuchu
open Hopac
open Fakta.Vault

let consulConfig = FaktaConfig.ConsulEmpty

let initVault (trm: bool) =
  let reqJson : InitRequest =
         {secretShares = 1
          secretThreshold =1
          pgpKeys = []}
  let state = FaktaState.empty APIType.Vault "" []
  let req = run (Fakta.Vault.Init.Init state (reqJson, []))
  match req with 
  | Choice1Of2 r -> FaktaState.empty APIType.Vault r.rootToken r.keys
  | Choice2Of2 _ -> FaktaState.empty APIType.Vault "" []


let logger =
  { new Logger with
      member x.log message =
        printfn "%A" message
        Alt.always (Promise.Now.withValue ())

      member x.logVerbose evaluate =
        printfn "%A" (evaluate ())
        Alt.always (Promise.Now.withValue ())

      member x.logSimple message =
        printfn "%A" message
    }

let state =
  { config = consulConfig
    logger = logger// NoopLogger
    clock  = SystemClock.Instance 
    random = Random () }

let ensureSuccess computation kontinue =
  match run computation with
  | Choice1Of2 x ->
    kontinue x

  | Choice2Of2 err ->
    Tests.failtestf "got error %A" err

let given value = ensureSuccess value ignore