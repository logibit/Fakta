module Fakta.Health

/// Checks is used to return the checks associated with a service
let checks (state : FaktaState) (service : string) (opts : QueryOptions) : Async<Choice<HealthCheck list * QueryMeta, Error>> =
  raise (TBD "not used")

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
  