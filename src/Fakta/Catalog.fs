module Fakta.Catalog

/// Datacenters is used to query for all the known datacenters
let datacenters (state : FaktaState) : Async<Choice<string list, Error>> =
  raise (TBD "not used")

/// 
let deregister (state : FaktaState) (dereg : CatalogDeregistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used")

/// Node is used to query for service information about a single node
let node (state : FaktaState) (node : string) (opts : QueryOptions) : Async<Choice<CatalogNode * QueryMeta, Error>> =
  raise (TBD "not used")

/// Nodes is used to query all the known nodes
let nodes (state : FaktaState) (opts : QueryOptions) : Async<Choice<Node list * QueryMeta, Error>> =
  raise (TBD "not used")

///
let register (state : FaktaState) (reg : CatalogRegistration) (opts : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "not used")

/// Service is used to query catalog entries for a given service
let service (state : FaktaState) (service : string) (tag : string) (opts : QueryOptions) : Async<Choice<CatalogService list * QueryMeta, Error>> =
  raise (TBD "not used")

/// Service is used to query catalog entries for a given service
let services (state : FaktaState) (opts : QueryOptions) : Async<Choice<Map<string, string list> * QueryMeta, Error>> =
  raise (TBD "not used")
  