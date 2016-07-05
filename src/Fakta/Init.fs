module Fakta.Vault.Init

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal initPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "init"; funcName |]

let internal queryFilters state =
  initPath >> queryFiltersNoMeta state

let internal writeFilters state =
  initPath >> writeFilters state

/// Leader is used to query for a known leader
let initStatus state: QueryCallNoMeta<Map<string, bool>> =
  let createRequest =
    queryCall state.config "sys/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "initStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let init state: WriteCallNoMeta<InitRequest, InitResponse> =     
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