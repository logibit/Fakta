module Fakta.Vault.Audit

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let auditPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "audit"; funcName |]

let queryFilters state =
  auditPath >> queryFiltersNoMeta state

let writeFilters state =
  auditPath >> writeFilters state

let AuditList state: QueryCallNoMeta<Map<string, Audit>> =
  let createRequest =
    queryCall state.config "sys/audit"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "auditList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let EnableAudit state : WriteCallNoMeta<Audit, unit> = 
  let createRequest (audit, opts) =
    writeCallUri state.config ("sys/audit/"+audit.Path.Value) opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT audit
    

  let filters =
    writeFilters state "enableAudit"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let DisableAudit state : WriteCallNoMeta<string, unit> =
  let createRequest (path, opts) =
    writeCallUri state.config ("sys/audit/" + path) opts
    |> basicRequest state.config Delete
    |> withVaultHeader state.config

  let filters =
    writeFilters state "disableAudit"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let HashAudit state : WriteCallNoMeta<string*Map<string,string>, Map<string,string>> = 
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