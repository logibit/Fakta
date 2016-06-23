module Fakta.Vault.Secrets

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let secretPath (funcName: string) =
  [| "Fakta"; "Vault"; "Secret"; funcName |]

let queryFilters state =
  secretPath >> queryFiltersNoMeta state

let writeFilters state =
  secretPath >> writeFilters state

let Read state: QueryCallNoMeta<string, SecretDataString> =
  let createRequest (path, opts) =
    queryCall state.config path opts
    |> withVaultHeader state.config

  let filters =
    queryFilters state "read"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let List state: QueryCallNoMeta<string, SecretDataList> =
  let createRequest (path, opts) =
    queryCall state.config path opts
    |> withVaultHeader state.config
    |> Request.queryStringItem "list" "true"

  let filters =
    queryFilters state "secretList"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let Write state: WriteCallNoMeta<(Map<string,string> * string), unit> =     
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

let WriteWithReturnValue state: WriteCallNoMeta<(Map<string,string> * string), SecretDataString> =     
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

let Delete state: WriteCallNoMeta<string, unit> =     
  let createRequest (path, opts) =
    writeCallUri state.config path opts
    |> basicRequest state.config Delete 
    |> withVaultHeader state.config

  let filters =
    writeFilters state "secretWrite"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters