module Fakta.Logging

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

/// The log levels specify the severity of the message.
[<CustomEquality; CustomComparison>]
type LogLevel =
  /// The most verbose log level, more verbose than Debug.
  | Verbose
  /// Less verbose than Verbose, more verbose than Info
  | Debug
  /// Less verbose than Debug, more verbose than Warn
  | Info
  /// Less verbose than Info, more verbose than Error
  | Warn
  /// Less verbose than Warn, more verbose than Fatal
  | Error
  /// The least verbose level. Will only pass through fatal
  /// log lines that cause the application to crash or become
  /// unusable.
  | Fatal
  with
    /// Convert the LogLevel to a string
    override x.ToString () =
      match x with
      | Verbose -> "verbose"
      | Debug -> "debug"
      | Info -> "info"
      | Warn -> "warn"
      | Error -> "error"
      | Fatal -> "fatal"

    /// Converts the string passed to a Loglevel.
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    static member FromString str =
      match str with
      | "verbose" -> Verbose
      | "debug" -> Debug
      | "info" -> Info
      | "warn" -> Warn
      | "error" -> Error
      | "fatal" -> Fatal
      | _ -> Info

    /// Turn the LogLevel into an integer
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    member x.ToInt () =
      (function
      | Verbose -> 1
      | Debug -> 2
      | Info -> 3
      | Warn -> 4
      | Error -> 5
      | Fatal -> 6) x

    /// Turn an integer into a LogLevel
    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    static member FromInt i =
      (function
      | 1 -> Verbose
      | 2 -> Debug
      | 3 -> Info
      | 4 -> Warn
      | 5 -> Error
      | 6 -> Fatal
      | _ as i -> failwithf "rank %i not available" i) i

    static member op_LessThan (a, b) = (a :> IComparable<LogLevel>).CompareTo(b) < 0
    static member op_LessThanOrEqual (a, b) = (a :> IComparable<LogLevel>).CompareTo(b) <= 0
    static member op_GreaterThan (a, b) = (a :> IComparable<LogLevel>).CompareTo(b) > 0
    static member op_GreaterThanOrEqual (a, b) = (a :> IComparable<LogLevel>).CompareTo(b) >= 0

    override x.Equals other = (x :> IComparable).CompareTo other = 0

    override x.GetHashCode () = x.ToInt ()

    interface IComparable with
      member x.CompareTo other =
        match other with
        | null -> 1
        | :? LogLevel as tother ->
          (x :> IComparable<LogLevel>).CompareTo tother
        | _ -> failwith <| sprintf "invalid comparison %A to %A" x other

    interface IComparable<LogLevel> with
      member x.CompareTo other =
        compare (x.ToInt()) (other.ToInt())

    interface IEquatable<LogLevel> with
      member x.Equals other =
        x.ToInt() = other.ToInt()

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
      member x.logVerbose evaluate = Alt.always (Promise.Now.withValue ())
      member x.log message = Alt.always (Promise.Now.withValue ())
      member x.logSimple message = () }

let private logger =
  ref ((fun () -> SystemClock.Instance.Now), fun (name : string) -> NoopLogger)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Message =

  let create (clock : IClock) path level fields message =
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

  let log (logger : Logger) message =
    logger.log message

  let logVerbose (logger : Logger) evaluate =
    logger.logVerbose evaluate

  let logSimple (logger : Logger) message =
    logger.logSimple message
