module Fakta.Vault.Auth

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal authPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "auth"; funcName |]

let internal queryFilters state =
  authPath >> queryFiltersNoMeta state

let internal writeFilters state =
  authPath >> writeFilters state


let authList state: QueryCallNoMeta<Map<string, AuthMount>> =
  let createRequest =
    queryCall state.config "sys/auth"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "authList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let authEnable state : WriteCallNoMeta<Map<string, string>*string, unit> = 
  let createRequest ((map, mountPoint), opts) =
    writeCallUri state.config ("sys/auth/"+mountPoint) opts
    |> basicRequest state.config Post 
    |> withVaultHeader state.config
    |> withJsonBodyT map    

  let filters =
    writeFilters state "authEnable"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let authDisable state : WriteCallNoMeta<string, unit> = 
  let createRequest (path, opts) =
    writeCallUri state.config ("sys/auth/"+path) opts
    |> basicRequest state.config Delete 
    |> withVaultHeader state.config

  let filters =
    writeFilters state "authDisable"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
