module Fakta.Vault.Capabilities

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let capabilitiesPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "capabilities"; funcName |]

let queryFilters state =
  capabilitiesPath >> queryFiltersNoMeta state

let writeFilters state =
  capabilitiesPath >> writeFilters state


let Capabilities state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
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


let CapabilitiesSelf state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
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

let CapabilitiesAccessor state : WriteCallNoMeta<Map<string, string>, Map<string, string list>> = 
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

