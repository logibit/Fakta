module Fakta.Vault.Health

open Fakta
open Fakta.Impl


let internal healthPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "health"; funcName |]

let internal queryFilters state =
  healthPath >> queryFiltersNoMeta state

let internal writeFilters state =
  healthPath >> writeFilters state

let getHealth state: QueryCallNoMeta<HealthResponse> =
  let createRequest =
    queryCall state.config "sys/health"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "getVaultHealth"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters