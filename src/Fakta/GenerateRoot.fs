module Fakta.Vault.GenerateRoot

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal genRootPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "generate-root"; funcName |]

let internal queryFilters state =
  genRootPath >> queryFiltersNoMeta state

let internal writeFilters state =
  genRootPath >> writeFilters state


let status state: QueryCallNoMeta<GenerateRootStatusResponse> =
  let createRequest =
    queryCall state.config "sys/generate-root/attempt"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "status"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters 


let init state: WriteCallNoMeta<string * string, GenerateRootStatusResponse> =
  let createRequest ((key, value), opts) =
    writeCallUri state.config "sys/generate-root/attempt" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add(key, value))

  let filters =
    writeFilters state "init"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let cancel state: WriteCallNoMeta<unit> =
  let createRequest  =
    writeCallUri state.config "sys/generate-root/attempt" 
    >> basicRequest state.config Delete

  let filters =
    writeFilters state "cancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let update state: WriteCallNoMeta<string * string, GenerateRootStatusResponse> =
  let createRequest ((key, nonce), opts)  =
    writeCallUri state.config "sys/generate-root/update" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("key", key).Add("nonce", nonce))

  let filters =
    writeFilters state "update"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters