﻿module Fakta.Vault.Init

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let statusPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let queryFilters state =
  statusPath >> queryFiltersNoMeta state

let writeFilters state =
  statusPath >> writeFilters state

/// Leader is used to query for a known leader
let InitStatus state: QueryCallNoMeta<Map<string, bool>> =
  let createRequest =
    queryCall state.config "sys/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "initStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

//WIP
let Init state: WriteCallNoMeta<InitRequest, InitResponse> =     
  let createRequest (reqJson, opts) =
    writeCallUri state.config "sys/init" opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT reqJson
    

  let filters =
    writeFilters state "init"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters