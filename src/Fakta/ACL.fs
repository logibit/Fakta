module Fakta.ACL
open Fakta
open Fakta.Logging
open Fakta.Impl
open System
open System.Collections
open NodaTime
open HttpFs.Client
open Chiron

let faktaAclString = "Fakta.acl"

let aclDottedPath (funcName: string) =
  (sprintf "%s.%s" faktaAclString funcName)


/// Clone is used to return a new token cloned from an existing one
let clone (state : FaktaState) (tokenID : Id) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =    
    let urlPath = (sprintf "clone/%s" tokenID)
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let result = Async.RunSynchronously (call state (aclDottedPath "clone") id uriBuilder HttpMethod.Put)
    async {
      match result with 
      | Choice1Of2 x -> 
          let body, (dur:Duration, resp:Response) = x
          let item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
          return Choice1Of2 (item.["ID"], writeMeta dur)
      | Choice2Of2 err -> return Choice2Of2(err)
    }
  

/// Create is used to generate a new token with the given parameters
let create (state : FaktaState) (tokenToCreate : ACLEntry) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
    let urlPath = "create"
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let json = Json.serialize tokenToCreate |> Json.format
    let result = Async.RunSynchronously (call state (aclDottedPath urlPath) (withJsonBody json) uriBuilder HttpMethod.Put)
    async {
        match result with 
        | Choice1Of2 x -> 
            let body, (dur:Duration, resp:Response) = x
            let item = if body = "" then Map.empty else Json.deserialize (Json.parse body)
            return Choice1Of2 (item.["ID"], writeMeta dur)
        | Choice2Of2 err -> return Choice2Of2(err)
      }

/// Destroy is used to destroy a given ACL token ID 
let destroy (state : FaktaState) (tokenID : Id) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
    let urlPath = (sprintf "destroy/%s" tokenID)
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let result = Async.RunSynchronously (call state (aclDottedPath "destroy") id uriBuilder HttpMethod.Put)
    async {
      match result with 
      | Choice1Of2 x -> 
          let _, (dur:Duration, _) = x
          return Choice1Of2 (writeMeta dur)
      | Choice2Of2 err -> return Choice2Of2(err)
    }

/// Info is used to query for information about an ACL token 
let info (state : FaktaState) (tokenID : Id) (opts : QueryOptions) : Async<Choice<ACLEntry * QueryMeta, Error>> =
    let urlPath = (sprintf "info/%s" tokenID)
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let result = Async.RunSynchronously (call state (aclDottedPath "info") id uriBuilder HttpMethod.Get)
    async {
        match result with 
        | Choice1Of2 x -> 
            let body, (dur:Duration, resp:Response) = x
            let items = if body = "" then [] else Json.deserialize (Json.parse body)
            return Choice1Of2 (items.[0], queryMeta dur resp)
        | Choice2Of2 err -> return Choice2Of2(err)
      }


/// List is used to get all the ACL tokens 
let list (state : FaktaState) (opts : QueryOptions) : Async<Choice<ACLEntry list * QueryMeta, Error>> =
    let urlPath = "list"
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let result = Async.RunSynchronously (call state (aclDottedPath urlPath) id uriBuilder HttpMethod.Get)
    async {
        match result with 
        | Choice1Of2 x -> 
            let body, (dur:Duration, resp:Response) = x
            let items = if body = "" then [] else Json.deserialize (Json.parse body)
            return Choice1Of2 (items, queryMeta dur resp)
        | Choice2Of2 err -> return Choice2Of2(err)
      }

/// Update is used to update the rules of an existing token
let update (state : FaktaState) (acl : ACLEntry) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
    let urlPath = "update"
    let uriBuilder = UriBuilder.ofAcl state.config urlPath
    let json = Json.serialize acl |> Json.format
    let result = Async.RunSynchronously (call state (aclDottedPath urlPath) (withJsonBody json) uriBuilder HttpMethod.Put)
    async {
        match result with 
        | Choice1Of2 x -> 
            let body, (dur:Duration, resp:Response) = x
            return Choice1Of2 (writeMeta dur)
        | Choice2Of2 err -> return Choice2Of2(err)
    }

