module Fakta.Vault.Keys

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let keysPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let queryFilters state =
  keysPath >> queryFiltersNoMeta state

let writeFilters state =
  keysPath >> writeFilters state

let KeyStatus state : QueryCallNoMeta<KeyStatus> =
  let createRequest =
    queryCall state.config "sys/key-status"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "keyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let Rotate state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rotate" 
    >> basicRequest state.config Post
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rotate"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let RekeyStatus state : QueryCallNoMeta<RekeyStatusResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters  


let RekeyInit state : WriteCallNoMeta<RekeyInitRequest, RekeyStatusResponse> =
  let createRequest (reqJson, opts) =
    writeCallUri state.config "sys/rekey/init" opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT reqJson

  let filters =
    writeFilters state "rekeyInit"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let RekeyCancel state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/init" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyCancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let RekeyUpdate state : WriteCallNoMeta<Map<string, string>, RekeyUpdateResponse> =
  let createRequest (respJson, opts) =
    writeCallUri state.config "sys/rekey/update" opts
    |> basicRequest state.config Put
    |> withVaultHeader state.config
    |> withJsonBodyT respJson

  let filters =
    writeFilters state "rekeyUpdate"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let RekeyRetrieveBackup state : QueryCallNoMeta<RekeyRetrieveResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/backup"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyBackupRetrieve"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let RekeyDeleteBackup state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/backup" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyBackup"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters  

let RekeyDeleteRecoveryBackup state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/recovery-backup" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyRecoveryBackup"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters 

let RekeyRecoveryKeyRetrieveBackup state : QueryCallNoMeta<RekeyRetrieveResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/recovery-backup"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyRecoveryKeyBackupRetrieve"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters
  

let RekeyRecoveryKeyUpdate state : WriteCallNoMeta<Map<string, string>, RekeyUpdateResponse> =
  let createRequest (respJson, opts) =
    writeCallUri state.config "sys/rekey-recovery-key/update" opts
    |> basicRequest state.config Put
    |> withVaultHeader state.config
    |> withJsonBodyT respJson

  let filters =
    writeFilters state "rekeyRecoveryKeyUpdate"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let RekeyRecoveryKeyCancel state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey-recovery-key/init" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyRecoveryKeyCancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
  

let RekeyRecoveryKeyInit state : WriteCallNoMeta<RekeyInitRequest, RekeyStatusResponse> =
  let createRequest (reqJson, opts) =
    writeCallUri state.config "sys/rekey-recovery-key/init" opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT reqJson

  let filters =
    writeFilters state "rekeyInit"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters
  

let RekeyRecoveryKeyStatus state : QueryCallNoMeta<RekeyStatusResponse> =
  let createRequest =
    queryCall state.config "sys/rekey-recovery-key/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyRecoveryKeyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters