[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory 

open System
open NodaTime
open HttpFs.Client
open Fakta
open Fakta.Logging
open Fuchu
open Hopac

let consulConfig = FaktaConfig.consulEmpty

let logger =
  { new Logger with
      member x.logWithAck logLevel msgFactory =
        printfn "%A" (msgFactory logLevel)
        async.Return ()

      member x.log level message =
        printfn "%A" message

      member x.logSimple message =
        printfn "%A" message
    }

module Message =
  open Fakta.Logging.Message

  let sprintf data =
    Printf.kprintf (event data)

let initVault =
  let reqJson : InitRequest =
    { secretShares = 1
      secretThreshold =1
      pgpKeys = []}

  let state =
    FaktaState.create APIType.Vault "" [] logger HttpFsState.empty

  let req =
    run (Fakta.Vault.Init.init state (reqJson, []))

  match req with 
  | Choice1Of2 r ->
    FaktaState.create APIType.Vault r.rootToken r.keys logger HttpFsState.empty

  | Choice2Of2 _ ->
    state

let initState = initVault
let vaultState = FaktaState.create APIType.Vault "" [] logger

let state =
  { config = consulConfig
    logger = logger// NoopLogger
    clock  = SystemClock.Instance 
    random = Random ()
    clientState = HttpFsState.empty }

let ensureSuccess computation kontinue =
  match run computation with
  | Choice1Of2 x ->
    kontinue x

  | Choice2Of2 err ->
    Tests.failtestf "got error %A" err

let given value = ensureSuccess value ignore
