#I "../../packages/Hopac/lib/net45"
#I "../../packages/Hopac.Extras/lib/net45"

#r "Hopac.Core.dll"
#r "Hopac.dll"
#r "Hopac.Platform.dll"
#r "Hopac.Extras.dll"

open System
open Hopac.Extensions
open Hopac.Extras
open Hopac.Timer.Global
open Hopac.Alt.Infixes
open Hopac.Job.Infixes
open Hopac.Infixes
open Hopac

type Request<'a> =
  | Get
  | Put of 'a
(*
type Cell<'a> = {
  reqCh: Ch<Request<'a>>
  replyCh: Ch<'a>
}

let put (c: Cell<'a>) (x: 'a) : Job<unit> = job {
  return! Ch.give c.reqCh (Put x)
}

let get (c: Cell<'a>) : Job<'a> = job {
  do! Ch.give c.reqCh Get
  return! Ch.take c.replyCh
}

let cell (x: 'a) : Job<Cell<'a>> = job {
  let c = {reqCh = Ch.Now.create (); replyCh = Ch.Now.create ()}
  let rec server x = job {
        let! req = Ch.take c.reqCh
        match req with
         | Get ->
           do! Ch.give c.replyCh x
           return! server x
         | Put x ->
           return! server x
      }
  do! Job.start (server x)
  return c
}
*)