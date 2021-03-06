﻿[<AutoOpen>]
module Fakta.IntegrationTests.TestFactory

open System
open NodaTime
open Expecto
open Hopac
open Hopac.Infixes
open HttpFs.Client
open Fakta
open Fakta.Logging
open Fakta.Logging.Message

module Message =
  let sprintf level =
    Printf.kprintf (event level)

type Microsoft.FSharp.Control.AsyncBuilder with
  member __.Bind(t: #Job<'T>, f:'T -> Async<'R>): Async<'R> =
    async.Bind(Job.toAsync t, f)

let consulConfig = FaktaConfig.consulEmpty

let logger =
  Log.create "Fakta.IntegrationTests"

let initVault =
  let reqJson: InitRequest =
    { secretShares = 1
      secretThreshold = 1
      pgpKeys = []}

  let state =
    FaktaState.create APIType.Vault "" [] logger HttpFsState.empty

  Fakta.Vault.Init.init state (reqJson, []) >>- function
  | Choice1Of2 r ->
    FaktaState.create APIType.Vault r.rootToken r.keys logger HttpFsState.empty
  | Choice2Of2 _ ->
    state

let initState = memo initVault
let vaultState = FaktaState.create APIType.Vault "" [] logger HttpFsState.empty

let state =
  { config = consulConfig
    logger = logger // NoopLogger
    clock = SystemClock.Instance
    random = Random ()
    clientState = HttpFsState.empty }

let ensureSuccess computation kontinue =
  computation >>- function
  | Choice1Of2 x ->
    kontinue x
  | Choice2Of2 err ->
    failtestf "Test failed with error %A" err
let ensureSuccessB computation kontinue =
  computation >>= function
    | Choice1Of2 res ->
      kontinue res
    | Choice2Of2 err ->
      failtestf "Test failed with error %A" err
let ensureSuccessA computation kontinue =
  ensureSuccessB computation (fun input -> Job.fromAsync (kontinue input))

let given value = ensureSuccess value ignore

let printResult (r: 'res) =
  Job.fromAsync (
    logger.logWithAck Info (eventX "Got {result}" >> setField "result" r)
  )