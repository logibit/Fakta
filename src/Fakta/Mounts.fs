module Fakta.Vault.Mounts

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal mountsPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "mounts"; funcName |]

let internal queryFilters state =
  mountsPath >> queryFiltersNoMeta state

let internal writeFilters state =
  mountsPath >> writeFilters state

let mounts state : QueryCallNoMeta<Map<string, MountOutput>> =
  let createRequest =
    queryCall state.config "sys/mounts"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "mounts"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let mount state: WriteCallNoMeta<(string * MountInput), unit> =
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


let tuneMount state : WriteCallNoMeta<(string * MountConfigInput), unit> =
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


let unmount state: WriteCallNoMeta<string, unit> =
  let createRequest (mountPoint, opts)  =
    writeCallUri state.config (sprintf "sys/mounts/%s" mountPoint) opts
    |> basicRequest state.config Delete
    |> withVaultHeader state.config

  let filters =
    writeFilters state "unmount"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let remount state : WriteCallNoMeta<(string * string), unit> =
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
