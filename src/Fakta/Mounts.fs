module Fakta.Vault.Mounts

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let mountsPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "mounts"; funcName |]

let queryFilters state =
  mountsPath >> queryFiltersNoMeta state

let writeFilters state =
  mountsPath >> writeFilters state

let Mounts state : QueryCallNoMeta<Map<string, MountOutput>> =
  let createRequest =
    queryCall state.config "sys/mounts"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "mounts"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let Mount state: WriteCallNoMeta<(string * MountInput), unit> =
  let createRequest ((mountPoint, mountConf), opts)  =
    writeCallUri state.config (sprintf "sys/mounts/%s" mountPoint) opts
    |> basicRequest state.config Post
    |> withVaultHeader state.config
    |> withJsonBodyT mountConf

  let filters =
    writeFilters state "mount"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let TuneMount state : WriteCallNoMeta<(string * MountConfigInput), unit> =
  let createRequest ((name, mountConfig), opts) =
    writeCallUri state.config (sprintf "sys/mounts/%s/tune" name) opts
    |> basicRequest state.config Post
    |> withVaultHeader state.config
    |> withJsonBodyT mountConfig

  let filters =
    writeFilters state "mounts"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let Unmount state: WriteCallNoMeta<string, unit> =
  let createRequest (mountPoint, opts)  =
    writeCallUri state.config (sprintf "sys/mounts/%s" mountPoint) opts
    |> basicRequest state.config Delete
    |> withVaultHeader state.config

  let filters =
    writeFilters state "unmount"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let Remount state : WriteCallNoMeta<(string * string), unit> =
  let createRequest ((fromMP, toMP), opts) =
    writeCallUri state.config "sys/remount" opts
    |> basicRequest state.config Post
    |> withVaultHeader state.config
    |> withJsonBodyT (Map.empty.Add("from", fromMP).Add("to", toMP))

  let filters =
    writeFilters state "remount"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
