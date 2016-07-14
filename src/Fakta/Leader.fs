module Fakta.Vault.Leader

open Fakta
open Fakta.Impl


let internal leaderPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "leader"; funcName |]

let internal queryFilters state =
  leaderPath >> queryFiltersNoMeta state

let leader state: QueryCallNoMeta<LeaderResponse> =
  let createRequest =
    queryCall state.config "sys/leader"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "leader"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

