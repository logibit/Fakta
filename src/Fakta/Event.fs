module Fakta.Event

open System

/// Fire is used to fire a new user event. Only the Name, Payload and Filters are respected. This returns the ID or an associated error. Cross DC requests are supported.
let fire (state : FaktaState) (events : UserEvent list) (opts : WriteOptions) : Async<Choice<string * WriteMeta, Error>> =
  raise (TBD "not used")

/// IDToIndex is a bit of a hack. This simulates the index generation to convert an event ID into a WaitIndex.
let idToIndex (state : FaktaState) (uuid : Guid) : uint64 =
  raise (TBD "not used")

/// List is used to get the most recent events an agent has received. This list can be optionally filtered by the name. This endpoint supports quasi-blocking queries. The index is not monotonic, nor does it provide provide LastContact or KnownLeader. 
let list (name : string) (opts : QueryOptions) : Async<Choice<UserEvent list * QueryMeta, Error>> =
  raise (TBD "not used")