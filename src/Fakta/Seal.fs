module Fakta.Vault.Seal

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal sealPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let internal queryFilters state =
  sealPath >> queryFiltersNoMeta state

let internal writeFilters state =
  sealPath >> writeFilters state


let sealStatus state : QueryCallNoMeta<SealStatusResponse> =
  let createRequest =
    queryCall state.config "sys/seal-status"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "sealStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let seal state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCall state.config "sys/seal"
    >> withVaultHeader state.config

  let filters =
    writeFilters state "seal"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let unseal state  : WriteCallNoMeta<string, SealStatusResponse> =   
  let createRequest (key, opts) =
    writeCallUri state.config "sys/unseal" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("key", key))

  let filters =
    writeFilters state "unseal"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let resetUnsealProcess state  : WriteCallNoMeta<SealStatusResponse> =   
  let createRequest opts =
    writeCallUri state.config "sys/unseal" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("reset", true))

  let filters =
    writeFilters state "unseal"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


