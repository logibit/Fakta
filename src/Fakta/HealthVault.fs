module Fakta.Vault.Health

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let healthPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "health"; funcName |]

let queryFilters state =
  healthPath >> queryFiltersNoMeta state

let writeFilters state =
  healthPath >> writeFilters state

let GetHealth state: QueryCallNoMeta<HealthResponse> =
  let createRequest =
    queryCall state.config "sys/health"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "getVaultHealth"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters