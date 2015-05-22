module Fakta.Status

/// Leader is used to query for a known leader 
let leader (s : FaktaState) : Async<Choice<string, Error>> =
  raise (TBD "TODO")

/// Peers is used to query for a known raft peers 
let peers (s : FaktaState) : Async<Choice<string list, Error>> =
  raise (TBD "TODO")