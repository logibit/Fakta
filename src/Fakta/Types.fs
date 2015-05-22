[<AutoOpen>]
module Fakta.Types

open System
open System.Text
open NodaTime
open Chiron
open Chiron.Operators

open Fakta.Logging

/// ACLCLientType is the client type token
let ACLClientType = "client"

/// ACLManagementType is the management type token
let ACLManagementType = "management"

////////////// LOCK
let DefaultLockWaitTime = Duration.FromSeconds 15L
let DefaultLockRetryTime = Duration.FromSeconds 5L
let LockFlagValue = 0x2ddccbc058a50c18UL

/// Tells the programmer this feature has not been implemented yet
exception TBD of reason:string

type HttpBasicAuth =
  { username : string
    password : string }

type Id = string

type Key = string

/// Token is used to provide a per-request ACL token
/// which overrides the agent's default token.
type Token = string

type Flags = uint64

type Index = uint64

type Session = string

type Port = uint16

// [{"CreateIndex":10,"ModifyIndex":17,"LockIndex":0,"Key":"fortnox/apikey","Flags":0,"Value":"MTMzOA=="}]
type KVPair =
    /// CreateIndex is the internal index value that represents when the entry was created.
  { createIndex : Index
    /// ModifyIndex is the last index that modified this key. This index corresponds to the X-Consul-Index header value that is returned in responses, and it can be used to establish blocking queries by setting the "?index" query parameter. You can even perform blocking queries against entire subtrees of the KV store: if "?recurse" is provided, the returned X-Consul-Index corresponds to the latest ModifyIndex within the prefix, and a blocking query using that "?index" will wait until any key within that prefix is updated.
    modifyIndex : Index
    /// LockIndex is the last index of a successful lock acquisition. If the lock is held, the Session key provides the session that owns the lock.
    lockIndex   : Index
    /// Key is simply the full path of the entry.
    key         : Key
    /// Flags are an opaque unsigned integer that can be attached to each entry. Clients can choose to use this however makes sense for their application.
    flags       : Flags
    /// Value is a Base64-encoded blob of data. Note that values cannot be larger than 512kB.
    value       : byte [] option }

  member x.hasFlags =
    x.flags <> 0UL

  static member Create(key : Key, value : byte []) =
    { createIndex = 0UL
      modifyIndex = 0UL
      lockIndex   = 0UL
      key         = key
      flags       = 0UL
      value       = Some value }

  static member Create(key : Key, value : string) =
    KVPair.Create(key, Encoding.UTF8.GetBytes value)

  static member FromJson (_ : KVPair) =
    (fun ci mi li k fl v ->
      { createIndex = ci
        modifyIndex = mi
        lockIndex   = li
        key         = k
        flags       = fl
        value       = v |> Option.map Convert.FromBase64String })
    <!> Json.read "CreateIndex"
    <*> Json.read "ModifyIndex"
    <*> Json.read "LockIndex"
    <*> Json.read "Key"
    <*> Json.read "Flags"
    <*> Json.read "Value"

  static member ToJson (kv : KVPair) =
    Json.write "CreateIndex" kv.createIndex
    *> Json.write "ModifyIndex" kv.modifyIndex
    *> Json.write "LockIndex" kv.lockIndex
    *> Json.write "Key" kv.key
    *> Json.write "Flags" kv.flags
    *> Json.write "Value" (kv.value |> Option.map Convert.ToBase64String)

type KVPairs = KVPair list
/// Allows you to pass a list that is built into a single Key (string) in the end.
type Keys = string list

type AgentService =
  { id      : Id
    service : string
    tags    : string list
    port    : Port
    address : string }

type HealthCheck =
  { node        : string
    checkId     : string
    name        : string
    status      : string
    notes       : string
    output      : string
    serviceId   : Id
    serviceName : string }

type LockOptions =
    /// Must be set and have write permissions
  { key         : Key
    /// Optional, value to associate with the lock
    value       : byte []
    /// Optional, created if not specified
    session     : Session
    /// Optional, defaults to DefaultLockSessionName
    sessionName : string
    /// Optional, defaults to DefaultLockSessionTTL
    sessionTTL  : string }

type Node =
  { node    : string
    address : string }

type ServiceEntry =
  { node    : Node
    service : AgentService
    checks  : HealthCheck list }

type SessionEntry =
  { createIndex : uint64
    id          : string
    name        : string
    node        : string
    checks      : string list
    lockDelay   : Duration
    behavior    : string
    ttl         : string }

type UserEvent =
  { id            : Id
    name          : string
    payload       : byte []
    nodeFilter    : string
    serviceFilter : string
    tagFilter     : string
    version       : int
    lTime         : uint64 }

type QueryMeta =
    /// LastIndex. This can be used as a WaitIndex to perform
    /// a blocking query
  { lastIndex   : Index
    /// Time of last contact from the leader for the
    /// server servicing the request
    lastContact : Duration
    /// Is there a known leader
    knownLeader : bool
    // How long did the request take
    requestTime : Duration }

///  QueryOptions are used to parameterize a query 
type QueryOptions =
    /// Providing a datacenter overwrites the DC provided
    /// by the Config
  { datacenter        : string option
    /// AllowStale allows any Consul server (non-leader) to service
    /// a read. This allows for lower latency and higher throughput
    allowStale        : bool
    /// RequireConsistent forces the read to be fully consistent.
    /// This is more expensive but prevents ever performing a stale
    /// read.
    requireConsistent : bool
    /// WaitIndex is used to enable a blocking query. Waits
    /// until the timeout or the next index is reached
    waitIndex         : Index option
    /// WaitTime is used to bound the duration of a wait.
    /// Defaults to that of the Config, but can be overriden.
    waitTime          : Duration option
    /// Token is used to provide a per-request ACL token
    /// which overrides the agent's default token.
    token             : Token option }

  member x.toKvs () =
    [ if x.datacenter |> Option.fold (fun s t -> t <> "") false then
        yield "dc", x.datacenter
      if x.token |> Option.fold (fun s t -> t <> "") false then
        yield "token", x.token
      if x.waitIndex |> Option.fold (fun s t -> t <> 0UL) false then
        yield "index", Some (x.waitIndex.ToString())
      if x.waitTime |> Option.fold (fun s t -> t <> Duration.Zero) false then
        yield "wait", Some (Duration.consulString (Option.get x.waitTime)) ]

type WriteMeta =
  { requestTime : Duration }

type WriteOptions =
  { datacenter : string option
    token      : Token option }

  member x.toKvs () =
    [ if x.datacenter |> Option.fold (fun s t -> t <> "") false then
        yield "dc", Option.get x.datacenter
      if x.token |> Option.fold (fun s t -> t <> "") false then
        yield "token", Option.get x.token ]

type Error =
  | Message of string
  | CASFailed

type FaktaConfig =
    /// The base URIs that we have servers at
  { serverBaseUris : Uri list
    /// Datacenter to use. If not provided, the default agent datacenter is used.
    datacenter     : string option
    /// HttpAuth is the auth info to use for http access.
    credentials    : HttpBasicAuth option
    /// WaitTime limits how long a Watch will block. If not provided,
    /// the agent default values will be used.
    waitTime       : Duration
    /// Token is used to provide a per-request ACL token
    /// which overrides the agent's default token.
    token          : Token option
  }
  static member Default =
    { serverBaseUris = [ Uri "http://127.0.0.1:8500" ]
      datacenter     = None
      credentials    = None
      waitTime       = DefaultLockWaitTime
      token          = None }

type QueryOptions with
  static member ofConfig (cfg : FaktaConfig) =
    { datacenter        = cfg.datacenter
      allowStale        = false
      requireConsistent = true
      waitIndex         = None
      waitTime          = Some cfg.waitTime
      token             = cfg.token }

type WriteOptions with
  static member ofConfig (cfg : FaktaConfig) =
    { datacenter = cfg.datacenter
      token      = cfg.token }

type FaktaState =
  { config : FaktaConfig
    logger : Logger
    clock  : IClock
    random : Random }