module Fakta.Vault.Policy

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let policyPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "policy"; funcName |]

let queryFilters state =
  policyPath >> queryFiltersNoMeta state

let writeFilters state =
  policyPath >> writeFilters state


let PoliciesList state: QueryCallNoMeta<string list> =
  let createRequest =
    queryCall state.config "sys/policy"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "policiesList"
    >> codec createRequest (ofJsonPrism (VaultResult.getProperty "keys"))

  HttpFs.Client.getResponse |> filters

let GetPolicy state: QueryCallNoMeta<string, string> =
  let createRequest (name, opts) =
    queryCall state.config ("sys/policy/"+name) opts
    |> withVaultHeader state.config

  let filters =
    queryFilters state "getPolicy"
    >> codec createRequest (ofJsonPrism (VaultResult.getProperty "rules"))

  HttpFs.Client.getResponse |> filters


let PutPolicy state : WriteCallNoMeta<Map<string, string>*string, unit> = 
  let createRequest ((map, path), opts) =
    writeCallUri state.config ("sys/policy/"+path) opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT map    

  let filters =
    writeFilters state "putPolicy"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let DeletePolicy state : WriteCallNoMeta<string, unit> = 
  let createRequest (path, opts) =
    writeCallUri state.config ("sys/policy/"+path) opts
    |> basicRequest state.config Delete 
    |> withVaultHeader state.config

  let filters =
    writeFilters state "deletePolicy"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
