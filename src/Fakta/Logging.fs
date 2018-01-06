module Fakta.LoggingXXX

open System
open System.Diagnostics
open NodaTime
open Hopac

let timeJob (runnable : Job<_>) : Job<'a * Duration> =
  job {
    let sw = Stopwatch.StartNew()
    let! res = runnable
    sw.Stop()
    return res, Duration.FromTicks sw.ElapsedTicks
  }
type Value =
  | Event of template:string
  | Gauge of value:float * units:string

/// When logging, write a Message like this with the source of your
/// log line as well as a message and an optional exception.
type Message =
  { /// the level that this log line has
    level     : LogLevel
    /// the source of the log line, e.g. 'ModuleName.FunctionName'
    path      : string[]
    /// the message that the application wants to log
    value     : Value
    /// Any key-value data pairs to log or interpolate into the message
    /// template.
    fields    : Map<string, obj>
    /// timestamp when this log line was created
    timestamp : Instant }

/// The primary Logger abstraction that you can log data into
type Logger =
  abstract logVerbose : (unit -> Message) -> Alt<Promise<unit>>
  abstract log : Message -> Alt<Promise<unit>>
  abstract logSimple : Message -> unit

let NoopLogger =
  { new Logger with
      member x.logVerbose evaluate = Alt.always (Promise(()))
      member x.log message = Alt.always (Promise(()))
      member x.logSimple message = () }

let private logger =
  ref ((fun () -> SystemClock.Instance.Now), fun (name: string) -> NoopLogger)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Message =

  let create (clock: IClock) path level fields message =
    { value     = Event message
      level     = level
      path      = path
      fields    = fields
      timestamp = clock.Now }

  let event fields message =
    { value     = Event message
      level     = Info
      path      = Array.empty
      fields    = fields |> Map.ofList
      timestamp = (fst !logger) () }

  let gauge value units =
    { value     = Gauge (value, units)
      level     = Debug
      path      = Array.empty
      fields    = Map.empty
      timestamp = (fst !logger) () }

  let setPath path message =
    { message with path = path }

  let setField name value message =
    { message with fields = message.fields |> Map.put name value }

  let timeJob path (runnable : Job<_>) =
    timeJob runnable |> Job.map (function
    | res, dur ->
      let msg = gauge (float dur.Ticks / float NodaConstants.TicksPerMillisecond) "ms"
                |> setPath path
      res, msg)

  let sprintf data =
    Printf.kprintf (event data)

let configure clock fLogger =
  logger := (clock, fLogger)

let getLoggerByName name =
  (!logger |> snd) name

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Logger =

  let log (logger: Logger) message =
    logger.log message

  let logVerbose (logger: Logger) evaluate =
    logger.logVerbose evaluate

  let logSimple (logger: Logger) message =
    logger.logSimple message
