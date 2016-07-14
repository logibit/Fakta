module Fakta.Vault.Stepdown

open Fakta
open Fakta.Impl
open HttpFs.Client


let internal stepdownPath (funcName: string) =
  [| "Fakta"; "Vault"; "Sys"; "stepdown"; funcName |]

let internal queryFilters state =
  stepdownPath >> queryFiltersNoMeta state

let internal writeFilters state =
  stepdownPath >> writeFilters state

let stepdown state: WriteCallNoMeta<unit> =     
  let createRequest =
    writeCallUri state.config "sys/step-down" 
    >> basicRequest state.config Put 
    

  let filters =
    writeFilters state "stepdown"
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters