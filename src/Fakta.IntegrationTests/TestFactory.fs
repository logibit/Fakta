[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory 

open System
open NodaTime
open Fakta
open Fakta.Logging
open Fuchu
open Hopac

let consulConfig = FaktaConfig.consulEmpty

let logger =
  { new Logger with
      member x.log message =
        printfn "%A" message
        Alt.always (Promise())

      member x.logVerbose evaluate =
        printfn "%A" (evaluate ())
        Alt.always (Promise())

      member x.logSimple message =
        printfn "%A" message
    }

let initVault =
  let reqJson : InitRequest =
    { secretShares = 1
      secretThreshold =1
      pgpKeys = []}
  let state = FaktaState.create APIType.Vault "" [] logger
  let req = run (Fakta.Vault.Init.init state (reqJson, []))
  match req with 
  | Choice1Of2 r -> FaktaState.create APIType.Vault r.rootToken r.keys logger
  | Choice2Of2 _ -> state

let initState = initVault
let vaultState = FaktaState.create APIType.Vault "" [] logger

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
