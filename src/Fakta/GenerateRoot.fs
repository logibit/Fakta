module Fakta.Vault.GenerateRoot

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let genRootPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "generate-root"; funcName |]

let queryFilters state =
  genRootPath >> queryFiltersNoMeta state

let writeFilters state =
  genRootPath >> writeFilters state


let Status state: QueryCallNoMeta<GenerateRootStatusResponse> =
  let createRequest =
    queryCall state.config "sys/generate-root/attempt"
    >> withVaultHeader state.config

  let filters =
    queryFilters state "status"
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters 


let Init state: WriteCallNoMeta<string * string, GenerateRootStatusResponse> =
  let createRequest ((key, value), opts) =
    writeCallUri state.config "sys/generate-root/attempt" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add(key, value))

  let filters =
    writeFilters state "init"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters

let Cancel state: WriteCallNoMeta<unit> =
  let createRequest  =
    writeCallUri state.config "sys/generate-root/attempt" 
    >> basicRequest state.config Delete

  let filters =
    writeFilters state "cancel"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters


let Update state: WriteCallNoMeta<string * string, GenerateRootStatusResponse> =
  let createRequest ((key, nonce), opts)  =
    writeCallUri state.config "sys/generate-root/update" opts
    |> basicRequest state.config Put
    |> withJsonBodyT (Map.empty.Add("key", key).Add("nonce", nonce))

  let filters =
    writeFilters state "update"
    >> respBodyFilter
    >> codec createRequest fstOfJsonNoMeta

  HttpFs.Client.getResponse |> filters