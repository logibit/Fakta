module Fakta.Health
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open NodaTime
open HttpFs.Client
open Chiron

/// Checks is used to return the checks associated with a service
let checks (state : FaktaState) (service : string) (opts : QueryOptions) : Async<Choice<HealthCheck list * QueryMeta, Error>> =
  let getResponse = Impl.getResponse state "Fakta.Health.checks"
  let req =
    UriBuilder.ofHealth state.config service
    |> flip UriBuilder.mappendRange (queryOptKvs opts)
    |> UriBuilder.uri
    |> basicRequest Get
    |> withConfigOpts state.config
  async {
  let! resp, dur = Duration.timeAsync (fun () -> getResponse req)
  match resp with
  | Choice1Of2 resp ->
    use resp = resp
    if not (resp.StatusCode = 200 || resp.StatusCode = 404) then
      return Choice2Of2 (Message (sprintf "unknown response code %d" resp.StatusCode))
    else
      match resp.StatusCode with
      | 404 -> return Choice2Of2 (Message service)
      | _ ->
        let! body = Response.readBodyAsString resp
        let items = if body = "" then [] else Json.deserialize (Json.parse body)
        return Choice1Of2 (items, queryMeta dur resp)

  | Choice2Of2 exx ->
    return Choice2Of2 (Error.ConnectionFailed exx)
}

/// Node is used to query for checks belonging to a given node
let node (state : FaktaState) (node : string) (opts : QueryOptions) : Async<Choice<HealthCheck list * QueryMeta, Error>> =
  raise (TBD "not used")

/// Service is used to query health information along with service info for a given service. It can optionally do server-side filtering on a tag or nodes with passing health checks only.
let service (state : FaktaState) (service : string) (tag : string)
            (passingOnly : bool) (opts : QueryOptions)
            : Async<Choice<ServiceEntry list * QueryMeta, Error>> =
  raise (TBD "not used")

let state (state : FaktaState) (statee : string) (opts : QueryOptions) : Async<Choice<ServiceEntry list * QueryMeta, Error>> =
  raise (TBD "not used")
  