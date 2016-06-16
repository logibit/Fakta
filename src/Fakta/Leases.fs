module Fakta.Vault.Leases

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let leasesPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let queryFilters state =
  leasesPath >> queryFiltersNoMeta state

let writeFilters state =
  leasesPath >> writeFilters state

//let Renew state: WriteCallNoMeta<(string*int), Secret> =
//  let createRequest ((id, increment), opts) =
//    writeCallEntityUri state.config "sys/renew" (id, opts)
//    |> basicRequest state.config Put 
//    |> withVaultHeader state.config
//    |> withJsonBodyT (Map.empty.Add("increment", increment))    
//
//  let filters =
//    writeFilters state "renew"
//    >> respBodyFilter
//    >> codec createRequest fstOfJsonNoMeta
//
//  HttpFs.Client.getResponse |> filters

let Revoke state: WriteCallNoMeta<string, unit> =
  let createRequest (id, opts) =
    writeCallEntityUri state.config "sys/renew" (id, opts)
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    

  let filters =
    writeFilters state "revoke"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let RevokePrefix state: WriteCallNoMeta<string, unit> =
  let createRequest (id, opts) =
    writeCallEntityUri state.config "sys/revoke-prefix" (id, opts)
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    

  let filters =
    writeFilters state "revoke"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let RevokeForce state: WriteCallNoMeta<string, unit> =
  let createRequest (id, opts) =
    writeCallEntityUri state.config "sys/revoke-force" (id, opts)
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    

  let filters =
    writeFilters state "revoke-force"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters