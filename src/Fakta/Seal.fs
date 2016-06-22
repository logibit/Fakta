module Fakta.Vault.Seal

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let sealPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let queryFilters state =
  sealPath >> queryFiltersNoMeta state

let writeFilters state =
  sealPath >> writeFilters state


let SealStatus state : QueryCallNoMeta<SealStatusResponse> =
  let createRequest =
    queryCall state.config "sys/seal-status"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "sealStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let Seal state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCall state.config "sys/seal"
    >> withVaultHeader state.config

  let filters =
    writeFilters state "seal"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let Unseal state  : WriteCallNoMeta<string, SealStatusResponse> =   
  let createRequest (key, opts) =
    writeCallUri state.config "sys/unseal" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("key", key))

  let filters =
    writeFilters state "unseal"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let ResetUnsealProcess state  : WriteCallNoMeta<SealStatusResponse> =   
  let createRequest opts =
    writeCallUri state.config "sys/unseal" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("reset", true))

  let filters =
    writeFilters state "unseal"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


