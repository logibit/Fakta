module Fakta.Session

open NodaTime

/// Create makes a new session. Providing a session entry can customize the session.
let create (s : FaktaState) (se : SessionEntry) (wo : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  raise (TBD "TODO")

/// CreateNoChecks is like Create but is used specifically to create a session with no associated health checks. 
let createNoChecks (s : FaktaState) (se : SessionEntry) (wo : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  raise (TBD "TODO")

/// Destroy invalides a given session 
let destroy (s : FaktaState) (se : SessionEntry) (wo : WriteOptions) : Async<Choice<WriteMeta, Error>> =
  raise (TBD "TODO")

/// Info looks up a single session 
let info (s : FaktaState) (id : string) (qo : QueryOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List gets all active sessions 
let list (s : FaktaState) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  raise (TBD "TODO")

/// List gets sessions for a node 
let node (s : FaktaState) (node : string) (qo : QueryOptions) : Async<Choice<SessionEntry list * QueryMeta, Error>> =
  raise (TBD "TODO")

/// Renew renews the TTL on a given session 
let renew (s : FaktaState) (id : string) (wo : WriteOptions) : Async<Choice<SessionEntry * QueryMeta, Error>> =
  raise (TBD "TODO")

/// RenewPeriodic is used to periodically invoke Session.Renew on a session until
/// a doneCh is closed. This is meant to be used in a long running goroutine
/// to ensure a session stays valid. 
let renewPeriodic (s : FaktaState) (initialTTL : Duration) (id : string) (wo : WriteOptions) : Async<Choice<unit, Error>> =
  //func (s *Session) RenewPeriodic(initialTTL string, id string, q *WriteOptions, doneCh chan struct{}) error
  raise (TBD "TODO")

