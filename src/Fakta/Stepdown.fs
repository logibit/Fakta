module Fakta.Vault.Stepdown

open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron
open Hopac


let stepdownPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "stepdown"; funcName |]

let queryFilters state =
  stepdownPath >> queryFiltersNoMeta state

let writeFilters state =
  stepdownPath >> writeFilters state

let Stepdown state: WriteCallNoMeta<unit> =     
  let createRequest =
    writeCallUri state.config "sys/step-down" 
    >> basicRequest state.config Put 
    

  let filters =
    writeFilters state "stepdown"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters