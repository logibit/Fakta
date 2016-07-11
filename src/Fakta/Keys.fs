module Fakta.Vault.Keys

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal keysPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; funcName |]

let internal queryFilters state =
  keysPath >> queryFiltersNoMeta state

let internal writeFilters state =
  keysPath >> writeFilters state

let keyStatus state : QueryCallNoMeta<KeyStatus> =
  let createRequest =
    queryCall state.config "sys/key-status"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "keyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters


let rotate state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rotate" 
    >> basicRequest state.config Post
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rotate"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let rekeyStatus state : QueryCallNoMeta<RekeyStatusResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters  


let rekeyInit state : WriteCallNoMeta<RekeyInitRequest, RekeyStatusResponse> =
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

let rekeyCancel state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/init" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyCancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let rekeyUpdate state : WriteCallNoMeta<Map<string, string>, RekeyUpdateResponse> =
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

let rekeyRetrieveBackup state : QueryCallNoMeta<RekeyRetrieveResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/backup"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyBackupRetrieve"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let rekeyDeleteBackup state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/backup" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyBackup"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters  

let rekeyDeleteRecoveryBackup state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey/recovery-backup" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyRecoveryBackup"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters 

let rekeyRecoveryKeyRetrieveBackup state : QueryCallNoMeta<RekeyRetrieveResponse> =
  let createRequest =
    queryCall state.config "sys/rekey/recovery-backup"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyRecoveryKeyBackupRetrieve"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters
  

let rekeyRecoveryKeyUpdate state : WriteCallNoMeta<Map<string, string>, RekeyUpdateResponse> =
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


let rekeyRecoveryKeyCancel state : WriteCallNoMeta<unit> =
  let createRequest =
    writeCallUri state.config "sys/rekey-recovery-key/init" 
    >> basicRequest state.config Delete
    >> withVaultHeader state.config

  let filters =
    writeFilters state "rekeyRecoveryKeyCancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters
  

let rekeyRecoveryKeyInit state : WriteCallNoMeta<RekeyInitRequest, RekeyStatusResponse> =
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
  

let rekeyRecoveryKeyStatus state : QueryCallNoMeta<RekeyStatusResponse> =
  let createRequest =
    queryCall state.config "sys/rekey-recovery-key/init"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "rekeyRecoveryKeyStatus"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters