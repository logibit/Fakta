module Fakta.Vault.Capabilities

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal capabilitiesPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "capabilities"; funcName |]

let internal queryFilters state =
  capabilitiesPath >> queryFiltersNoMeta state

let internal writeFilters state =
  capabilitiesPath >> writeFilters state


let capabilities state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
  let createRequest (map, opts) =
    writeCallUri state.config "sys/capabilities" opts
    |> basicRequest state.config Post 
    |> withVaultHeader state.config
    |> withJsonBodyT map
    

  let filters =
    writeFilters state "capabilitiesList"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let capabilitiesSelf state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
  let createRequest (map, opts) =
    writeCallUri state.config "sys/capabilities-self" opts
    |> basicRequest state.config Post 
    |> withVaultHeader state.config
    |> withJsonBodyT map
    

  let filters =
    writeFilters state "capabilitiesSelf"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let capabilitiesAccessor state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
  let createRequest (map, opts) =
    writeCallUri state.config "sys/capabilities-accessor" opts
    |> basicRequest state.config Post 
    |> withVaultHeader state.config
    |> withJsonBodyT map
    

  let filters =
    writeFilters state "capabilitiesAccessor"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

