module Fakta.ACL

/// Clone is used to return a new token cloned from an existing one
let clone (state : FaktaState) (id : Id) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  raise (TBD "not used yet")

/// Create is used to generate a new token with the given parameters
let create (state : FaktaState) (id : ACLEntry) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  raise (TBD "not used yet")

/// Destroy is used to destroy a given ACL token ID 
let destroy (state : FaktaState) (id : Id) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used yet")

/// Info is used to query for information about an ACL token 
let info (state : FaktaState) (id : Id) (opts : QueryOptions) : Async<Choice<ACLEntry * QueryMeta, Error>> =
  raise (TBD "not used yet")

/// List is used to get all the ACL tokens 
let list (state : FaktaState) (opts : QueryOptions) : Async<Choice<ACLEntry list * QueryMeta, Error>> =
  raise (TBD "not used yet")

/// Update is used to update the rules of an existing token
let update (state : FaktaState) (acl : ACLEntry) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used yet")