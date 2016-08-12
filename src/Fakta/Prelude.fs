[<AutoOpen>]
module internal Fakta.Prelude

[<assembly: System.Runtime.CompilerServices.InternalsVisibleTo "Fakta.Tests">]
()

module Duration =
  open Hopac
  open NodaTime
  open System.Diagnostics

  let fromStopwatch (sw : Stopwatch) =
    Duration.FromTimeSpan sw.Elapsed

  let time f =
    let sw = Stopwatch.StartNew()
    let res = f ()
    sw.Stop()
    res, fromStopwatch sw

  let timeJob (f : unit -> Job<'a>) =
    job {
      let sw = Stopwatch.StartNew()
      let! res = f ()
      sw.Stop()
      return res, fromStopwatch sw
    }

  let consulString (d : Duration) =
    sprintf "%d%s" (uint32 (d.ToTimeSpan().TotalSeconds)) "s"

open Hopac
open Fakta.Logging
open NodaTime

module Message =

  let timeJob path (runnable : Job<_>) =
    Duration.timeJob (fun () -> runnable) |> Job.map (function
    | res, dur ->
      let msg =
        Message.gauge (int64 (float dur.Ticks / float NodaConstants.TicksPerMillisecond)) "ms"
        |> Message.setName path
      res, msg)

open System

module Chiron =
  open Chiron
  module Json =
    let inline maybeWrite key value =
      match value with
      | None -> fun json -> Value (), json
      | _    -> Json.write key value

open NodaTime
open NodaTime.Text
open Chiron
open Chiron.Optics
open Chiron.Operators

type Duration with
  static member ParsedDuration dur =
    let parseResult = DurationPattern.CreateWithInvariantCulture("ss\s").Parse dur
    if parseResult.Success then
      parseResult.Value
    else
      Duration.Epsilon

  static member FromJson =
    (function
    | String s -> Value (Duration.ParsedDuration s)
    | json ->
      Json.formatWith JsonFormattingOptions.SingleLine json
      |> sprintf "Expected a string containing a valid duration: %s"
      |> Error)

  static member ToJson (dur : Duration) =
    Json.Optic.set Json.String_ (Duration.consulString dur)