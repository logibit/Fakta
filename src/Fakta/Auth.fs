module Fakta.Vault.Auth

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let authPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "auth"; funcName |]

let queryFilters state =
  authPath >> queryFiltersNoMeta state

let writeFilters state =
  authPath >> writeFilters state


let AuthList state: QueryCallNoMeta<Map<string, AuthMount>> =
  let createRequest =
    queryCall state.config "sys/auth"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "authList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let AuthEnable state : WriteCallNoMeta<Map<string, string>*string, unit> = 
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


let AuthDisable state : WriteCallNoMeta<string, unit> = 
  let createRequest (path, opts) =
    writeCallUri state.config ("sys/auth/"+path) opts
    |> basicRequest state.config Delete 
    |> withVaultHeader state.config

  let filters =
    writeFilters state "authDisable"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
