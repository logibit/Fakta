module Fakta.ACL

open System
open System.Collections
open NodaTime
open HttpFs.Client
open HttpFs.Composition
open Chiron
open Aether.Operators
open Hopac
open System.Diagnostics
open Fakta
open Fakta.Impl

// no warnings for lesser generalisation
#nowarn "64"

let aclPath (operation: string) =
  [| "Fakta"; "ACL"; operation |]

let writeFilters state operation =
  timerFilter state (aclPath operation)
  >> unknownsFilter
  >> exnsFilter

let readFilters state operation =
  timerFilter state (aclPath operation)
  >> unknownsFilter
  >> exnsFilter
  >> respQueryFilter

let codec prepare interpret : JobFilter<'a, Choice<'b, Error>, 'i, Choice<'o, _>> =
  JobFunc.mapLeft prepare
  >> JobFunc.map (Choice.bind interpret)

let hasNoRespBody _ =
  Choice.create ()

module ConsulResult =

  let objectId =
    Json.Object_
    >?> Aether.Optics.Map.key_ "ID"

  let firstObjectOfArray =
    Json.Array_
    >?> Aether.Optics.List.head_

let ofJsonPrism jsonPrism : string -> Choice<'a, Error> =
  Json.tryParse
  >> Choice.bind (Aether.Optic.get jsonPrism >> Choice.ofOption "expected property missing")
  >> Choice.bind Json.tryDeserialize
  >> Choice.mapSnd Error.Message

/// Convert the first value in the tuple in the choice to some type 'a.
let inline internal fstOfJsonPrism jsonPrism (item1, item2) : Choice<'a, Error> =
  Json.tryParse item1
  |> Choice.bind (Aether.Optic.get jsonPrism >> Choice.ofOption "expected property missing")
  |> Choice.bind Json.tryDeserialize
  |> Choice.map (fun x -> x, item2)
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg item1)
  |> Choice.mapSnd Error.Message

/// Convert the first value in the tuple in the choice to some type 'a.
let inline internal fstOfJson (item1, item2) : Choice<'a, Error> =
  Json.tryParse item1
  |> Choice.bind Json.tryDeserialize
  |> Choice.map (fun x -> x, item2)
  |> Choice.mapSnd (fun msg ->
    sprintf "Json deserialisation tells us this error: '%s'. Couldn't deserialise input:\n%s" msg item1)
  |> Choice.mapSnd Error.Message

///The clone endpoint must be hit with a PUT. It clones the ACL identified by the id portion of the path and returns a new token ID. This allows a token to serve as a template for others, making it simple to generate new tokens without complex rule management.
///
/// The request is automatically routed to the authoritative ACL datacenter. Requests to this endpoint must be made with a management token.
let clone (state : FaktaState) : WriteCall<Id, Id> =
  let createRequest =
    fun (entity, opts) -> string entity, opts
    >> writeCallEntityUri state.config "acl/clone"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "clone"
    >> respBodyFilter
    >> codec createRequest (ofJsonPrism ConsulResult.objectId)

  HttpFs.Client.getResponse |> filters

/// The create endpoint is used to make a new token. A token has a name, a type, and a set of ACL rules.
/// 
/// The Name property is opaque to Consul. To aid human operators, it should be a meaningful indicator of the ACL's purpose.
///
/// Type is either client or management. A management token is comparable to a root user and has the ability to perform any action including creating, modifying, and deleting ACLs.
///
/// By constrast, a client token can only perform actions as permitted by the rules associated. Client tokens can never manage ACLs. Given this limitation, only a management token can be used to make requests to the /v1/acl/create endpoint.
///
/// In any Consul cluster, only a single datacenter is authoritative for ACLs, so all requests are automatically routed to that datacenter regardless of the agent to which the request is made.
let create state : WriteCall<ACLEntry, Id> =
  let createRequest (entry, opts) =
    writeCallUri state.config "acl/create" opts
    |> basicRequest state.config Put
    |> withJsonBodyT entry

  let filters =
    writeFilters state "create"
    >> respBodyFilter
    >> codec createRequest (ofJsonPrism ConsulResult.objectId)

  HttpFs.Client.getResponse |> filters

///The destroy endpoint must be hit with a PUT. This endpoint destroys the ACL token identified by the id portion of the path.
///
/// The request is automatically routed to the authoritative ACL datacenter. Requests to this endpoint must be made with a management token.
let destroy state : WriteCall<Id, unit> =
  let createRequest =
    writeCallEntityUri state.config "acl/destroy"
    >> basicRequest state.config Put

  let filters =
    writeFilters state "destroy"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters

/// The info endpoint must be hit with a GET. This endpoint returns the ACL token information identified by the id portion of the path.
let info state : QueryCall<Id, ACLEntry> =
  let createRequest =
    queryCallEntityUri state.config "acl/info"
    >> basicRequest state.config Get

  let filters =
    readFilters state "info"
    >> codec createRequest (fstOfJsonPrism ConsulResult.firstObjectOfArray)

  HttpFs.Client.getResponse |> filters

/// The list endpoint must be hit with a GET. It lists all the active ACL tokens. This is a privileged endpoint and requires a management token.
let list state : QueryCall<ACLEntry list> =
  let createRequest =
    queryCallUri state.config "acl/list"
    >> basicRequest state.config Get

  let filters =
    readFilters state "list"
    >> codec createRequest fstOfJson

  HttpFs.Client.getResponse |> filters

/// The update endpoint is used to modify the policy for a given ACL token. It is very similar to the create endpoint; however, instead of generating a new token ID, the ID field must be provided. As with /v1/acl/create, requests to this endpoint must be made with a management token. If the ID does not exist, the ACL will be upserted. In this sense, create and update are identical.
///
/// In any Consul cluster, only a single datacenter is authoritative for ACLs, so all requests are automatically routed to that datacenter regardless of the agent to which the request is made.
///
/// Only the ID field is mandatory. The other fields provide defaults: the Name and Rules fields default to being blank, and Type defaults to "client". The format of Rules is documented here:
///
/// https://www.consul.io/docs/internals/acl.html
let update state : WriteCall<ACLEntry, unit> =
  let createRequest (entry, opts) =
    writeCallUri state.config "acl/update" opts
    |> basicRequest state.config Put
    |> withJsonBodyT entry

  let filters =
    writeFilters state "update"
    >> respBodyFilter
    >> codec createRequest hasNoRespBody

  HttpFs.Client.getResponse |> filters