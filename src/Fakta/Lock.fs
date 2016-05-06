module Fakta.Lock

open NodaTime

/// Release is used for a lock release operation. The Key, Flags, Value and
/// Session are respected. Returns true on success or false on failures.
let destroy (s : FaktaState) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")

/// Lock attempts to acquire the lock and blocks while doing so. Providing a
/// non-nil stopCh can be used to abort the lock attempt. Returns a channel that
/// is closed if our lock is lost or an error. This channel could be closed at
/// any time due to session invalidation, communication errors, operator
/// intervention, etc. It is NOT safe to assume that the lock is held until
/// Unlock() unless the Session is specifically created without any associated
/// health checks. By default Consul sessions prefer liveness over safety and an
/// application must be able to handle the lock being lost.
let lock (s : FaktaState) =
  raise (TBD "TODO") // func (l *Lock) Lock(stopCh <-chan struct{}) (<-chan struct{}, error)

/// Unlock released the lock. It is an error to call this if the lock is not currently held.
let unlock (s : FaktaState) : Async<Choice<unit, Error>> =
  raise (TBD "TODO")