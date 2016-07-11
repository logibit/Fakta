module Fakta.Vault.Secrets

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal secretPath (funcName: string) =
  [| "Fakta"; "Vault"; "Secret"; funcName |]

let internal queryFilters state =
  secretPath >> queryFiltersNoMeta state

let internal writeFilters state =
  secretPath >> writeFilters state

let read state: QueryCallNoMeta<string, SecretDataString> =
  let createRequest (path, opts) =
    queryCall state.config path opts
    |> withVaultHeader state.config

  let filters =
    queryFilters state "read"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let list state: QueryCallNoMeta<string, SecretDataList> =
  let createRequest (path, opts) =
    queryCall state.config path opts
    |> withVaultHeader state.config
    |> Request.queryStringItem "list" "true"

  let filters =
    queryFilters state "secretList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let write state: WriteCallNoMeta<(Map<string,string> * string), unit> =     
  let createRequest ((data, path), opts) =
    writeCallUri state.config path opts
    |> basicRequest state.config Put 
    |> withVaultHeader state.config
    |> withJsonBodyT data    

  let filters =
    writeFilters state "secretWrite"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

let writeWithReturnValue state: WriteCallNoMeta<(Map<string,string> * string), SecretDataString> =     
  let createRequest ((data, path), opts) =
    writeCallUri state.config path opts
    |> basicRequest state.config Post 
    |> withVaultHeader state.config
    |> withJsonBodyT data    

  let filters =
    writeFilters state "secretWriteReturnValue"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let delete state: WriteCallNoMeta<string, unit> =     
  let createRequest (path, opts) =
    writeCallUri state.config path opts
    |> basicRequest state.config Delete 
    |> withVaultHeader state.config

  let filters =
    writeFilters state "secretWrite"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters