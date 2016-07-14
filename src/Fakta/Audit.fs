module Fakta.Vault.Audit

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal auditPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "audit"; funcName |]

let internal queryFilters state =
  auditPath >> queryFiltersNoMeta state

let internal writeFilters state =
  auditPath >> writeFilters state

let auditList state: QueryCallNoMeta<Map<string, Audit>> =
  let createRequest =
    queryCall state.config "sys/audit"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "auditList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let enableAudit state : WriteCallNoMeta<Audit, unit> = 
  let createRequest (audit:Audit, opts) =
    writeCallUri state.config ("sys/audit/"+audit.path.Value) opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT audit
    

  let filters =
    writeFilters state "enableAudit"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let disableAudit state : WriteCallNoMeta<string, unit> =
  let createRequest (path, opts) =
    writeCallUri state.config ("sys/audit/" + path) opts
    |> basicRequest state.config Delete
    |> withVaultHeader state.config

  let filters =
    writeFilters state "disableAudit"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let hashAudit state : WriteCallNoMeta<string*Map<string,string>, Map<string,string>> = 
  let createRequest ((path, input), opts) =
    writeCallUri state.config ("sys/audit-hash/"+path) opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT input
    

  let filters =
    writeFilters state "hashAudit"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters