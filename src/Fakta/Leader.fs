module Fakta.Vault.Leader

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let leaderPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "leader"; funcName |]

let queryFilters state =
  leaderPath >> queryFiltersNoMeta state

let writeFilters state =
  leaderPath >> writeFilters state

let Leader state: QueryCallNoMeta<LeaderResponse> =
  let createRequest =
    queryCall state.config "sys/leader"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "leader"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

