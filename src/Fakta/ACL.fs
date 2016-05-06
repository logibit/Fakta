module Fakta.ACL
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open HttpFs.Composition
open Chiron
open Hopac
open System.Diagnostics

let faktaAclString = "Fakta.ACL"

let aclPath (funcName: string) =
  [| "Fakta"; "ACL"; funcName |]

// build uri
// make call
// decode to custom

type WriteCall<'i, 'o> = JobFunc<'i * WriteOptions, Choice<'o, Error>>
type QueryCall<'i, 'o> = JobFunc<'i * QueryOptions, Choice<'o, Error>>

let timerFilter (state : FaktaState) path : JobFilter<Request, Response> =
  fun next req -> job {
    let sw = Stopwatch.StartNew()
    let! res = next req
    sw.Stop()

    Message.gauge (float sw.ElapsedMilliseconds) "ms"
    |> Message.setPath path
    |> Message.setField "path" req.url.AbsolutePath
    |> Logger.logSimple state.logger

    return res
  }

let unknownsFilter : JobFilter<Request, Response, Request, Choice<Response, Error>> =
  fun next ->
    next >> Job.map (fun resp ->
      if not (resp.statusCode = 200 || resp.statusCode = 404) then
        Choice.createSnd (Message (sprintf "unknown response code %d" resp.statusCode))
      elif resp.statusCode = 404 then
        Choice.createSnd (Error.ResourceNotFound)
      else
        Choice.create resp
    )

let exnsFilter : JobFilter<Request, Choice<Response, Error>> =
  fun next req ->
    job {
      try
        return! next req
      with :? System.Net.WebException as e ->
        return Choice.createSnd (Error.ConnectionFailed e)
    }

let bodyFilter : JobFilter<Request, Choice<Response, Error>, Request, Choice<string, Error>> =
  fun next req ->
    next req
    |> Job.bind (function
       | Choice1Of2 resp -> resp |> Response.readBodyAsString |> Job.map Choice.create
       | Choice2Of2 error -> Job.result (Choice2Of2 error))

//let handleEmpty otherwise f body =
//  if body = "" then otherwise else f body

let filters state lastPathBit =
  timerFilter state (aclPath lastPathBit)
  >> unknownsFilter
  >> exnsFilter

let filtersBody state lastPathBit : JobFilter<_, _, Request, Choice<string, Error>> =
  filters state lastPathBit
  >> bodyFilter

let codec prepare interpret : JobFilter<'a, Choice<'b, Error>, 'i, Choice<'o, _>> =
  JobFunc.mapLeft prepare
  >> JobFunc.map (Choice.bind interpret)

let ofJson fMap =
  Json.tryParse
  >> Choice.bind Json.tryDeserialize
  >> Choice.map fMap
  >> Choice.mapSnd Error.Message

let clone (state : FaktaState) : WriteCall<Id, string> =
  let createRequest =
    writeCallUri state.config "clone"
    >> basicRequest state.config Put

  let filters =
    filtersBody state "clone"
    >> codec createRequest (ofJson (Map.find "ID"))

  getResponse |> filters

/// Create is used to generate a new token with the given parameters
let create (state : FaktaState) (tokenToCreate : ACLEntry) (opts : WriteOptions) : Job<Choice<string * WriteMeta, Error>> = job {
  let urlPath = "create"
  let uriBuilder = UriBuilder.ofAcl state.config urlPath
  let json = Json.serialize tokenToCreate |> Json.format
  let! result = call state (aclPath urlPath) (withJsonBody json) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
      return Choice1Of2 (item.["ID"], writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Destroy is used to destroy a given ACL token ID
let destroy (state : FaktaState) (tokenID : Id) (opts : WriteOptions) : Job<Choice<WriteMeta, Error>> = job {
  let urlPath = (sprintf "destroy/%s" tokenID)
  let uriBuilder = UriBuilder.ofAcl state.config urlPath
  let! result = call state (aclPath "destroy") id uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 (_, (dur, _)) ->
      return Choice1Of2 (writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Info is used to query for information about an ACL token
let info (state : FaktaState) (tokenID : Id) (opts : QueryOptions) : Job<Choice<ACLEntry * QueryMeta, Error>> = job {
  let urlPath = (sprintf "info/%s" tokenID)
  let uriBuilder = UriBuilder.ofAcl state.config urlPath
  let! result = call state (aclPath "info") id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items.[0], queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}


/// List is used to get all the ACL tokens
let list (state : FaktaState) (opts : QueryOptions) : Job<Choice<ACLEntry list * QueryMeta, Error>> = job {
  let urlPath = "list"
  let uriBuilder = UriBuilder.ofAcl state.config urlPath
  let! result = call state (aclPath urlPath) id uriBuilder HttpMethod.Get
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      let items = if body = "[]" then [] else Json.deserialize (Json.parse body)
      return Choice1Of2 (items, queryMeta dur resp)
  | Choice2Of2 err -> return Choice2Of2(err)
}

/// Update is used to update the rules of an existing token
let update (state : FaktaState) (acl : ACLEntry) (opts : WriteOptions) : Job<Choice<WriteMeta, Error>> = job {
  let urlPath = "update"
  let uriBuilder = UriBuilder.ofAcl state.config urlPath
  let json = Json.serialize acl |> Json.format
  let! result = call state (aclPath urlPath) (withJsonBody json) uriBuilder HttpMethod.Put
  match result with
  | Choice1Of2 (body, (dur, resp)) ->
      return Choice1Of2 (writeMeta dur)
  | Choice2Of2 err -> return Choice2Of2(err)
}