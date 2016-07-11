module Fakta.Semaphore

open NodaTime
open Hopac

/// Acquire attempts to reserve a slot in the semaphore, blocking until success, interrupted via the stopCh or an error is encounted. Providing a non-nil stopCh can be used to abort the attempt. On success, a channel is returned that represents our slot. This channel could be closed at any time due to session invalidation, communication errors, operator intervention, etc. It is NOT safe to assume that the slot is held until Release() unless the Session is specifically created without any associated health checks. By default Consul sessions prefer liveness over safety and an application must be able to handle the session being lost.
let acquire (s : FaktaState) : Job<Choice<unit, Error>> =
  // func (s *Semaphore) Acquire(stopCh <-chan struct{}) (<-chan struct{}, error)
  raise (TBDException "TODO")

/// Destroy is used to cleanup the semaphore entry. It is not necessary to invoke. It will fail if the semaphore is in use.
let destroy (s : FaktaState) : Job<Choice<unit, Error>> =
  raise (TBDException "TODO") // func (l *Lock) Lock(stopCh <-chan struct{}) (<-chan struct{}, error)

/// Release is used to voluntarily give up our semaphore slot. It is an error to call this if the semaphore has not been acquired.
let release (s : FaktaState) : Job<Choice<unit, Error>> =
  raise (TBDException "TODO")