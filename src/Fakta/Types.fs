[<AutoOpen>]
module Fakta.Types

open System
open System.Text
open NodaTime
open Aether
open Aether.Operators
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

/// Tells the programmer this feature has not been implemented yet.
exception TBD of reason:string

type HttpBasicAuth =
  { username : string
    password : string }

type Id = string

type Key = string

/// Token is used to provide a per-request ACL token which overrides the agent's
/// default token.
type Token = string

type Flags = uint64

type Index = uint64

type Session = string

type Port = uint16

type Check = string

type Node =
  { node    : string
    address : string }

type ACLEntry =
  { createIndex : Index
    modifyIndex : Index
    id          : Id
    name        : string
    ``type``    : string
    rules       : string }

type AgentCheck =
  { node        : string
    checkID     : string
    name        : string
    status      : string
    notes       : string
    output      : string
    serviceId   : string
    serviceName : string }

type AgentMember =
  { name        : string
    addr        : string
    port        : uint16
    tags        : Map<string, string>
    status      : int
    protocolMin : uint8
    protocolMax : uint8
    protocolCur : uint8
    delegateMin : uint8
    delegateMax : uint8
    delegateCur : uint8 }

type AgentService =
  { id      : Id
    service : string
    tags    : string list
    port    : Port
    address : string }

type AgentServiceCheck =
  { script   : string // `json:",omitempty"`
    interval : string // `json:",omitempty"`
    timeout  : string // `json:",omitempty"`
    ttl      : string // `json:",omitempty"`
    http     : string // `json:",omitempty"`
    status   : string  } // `json:",omitempty"`

type AgentServiceChecks = AgentServiceCheck list

type AgentServiceRegistration =
  { id      : string //   `json:",omitempty"`
    Name    : string //  `json:",omitempty"`
    Tags    : string list // `json:",omitempty"`
    Port    : int //     `json:",omitempty"`
    Address : string //  `json:",omitempty"`
    Check   : AgentServiceCheck
    checks  : AgentServiceChecks }

type AgentCheckRegistration =
  { id        : Id // `json:",omitempty"`
    name      : string //`json:",omitempty"`
    notes     : string // `json:",omitempty"`
    serviceId : Id // `json:",omitempty"`
    check     : AgentServiceCheck }

type CatalogDeregistration =
  { node       : string
    address    : string
    datacenter : string
    serviceId  : string
    checkId    : string }

type CatalogNode =
  { node     : Node
    Services : Map<string, AgentService> }

type CatalogRegistration =
  { Node       : string
    Address    : string
    Datacenter : string
    Service    : AgentService
    Check      : AgentCheck }

type CatalogService =
  { node           : string
    address        : string
    serviceID      : string
    serviceName    : string
    serviceAddress : string
    serviceTags    : string list
    servicePort    : int }

// [{"CreateIndex":10,"ModifyIndex":17,"LockIndex":0,"Key":"fortnox/apikey","Flags":0,"Value":"MTMzOA=="}]
type KVPair =
    /// CreateIndex is the internal index value that represents when the entry
    /// was created.
  { createIndex : Index
    /// ModifyIndex is the last index that modified this key. This index
    /// corresponds to the X-Consul-Index header value that is returned in responses, and it can be used to establish blocking queries by setting the "?index" query parameter. You can even perform blocking queries against entire subtrees of the KV store: if "?recurse" is provided, the returned X-Consul-Index corresponds to the latest ModifyIndex within the prefix, and a blocking query using that "?index" will wait until any key within that prefix is updated.
    modifyIndex : Index
    /// LockIndex is the last index of a successful lock acquisition. If the
    /// lock is held, the Session key provides the session that owns the lock.
    lockIndex   : Index
    /// Key is simply the full path of the entry.
    key         : Key
    /// Flags are an opaque unsigned integer that can be attached to each entry.
    /// Clients can choose to use this however makes sense for their application.
    flags       : Flags
    /// Value is a Base64-encoded blob of data. Note that values cannot be
    /// larger than 512kB.
    value       : byte []
    /// If this key has been acquired by a specific session, that session id is
    /// given here.
    session     : string option }

  member x.hasFlags =
    x.flags <> 0UL

  member x.utf8String =
    UTF8.toString x.value

  static member Create(key : Key, value : byte [], ?flags) =
    // TODO: validate that value <= 512 KiB
    { createIndex = 0UL
      modifyIndex = 0UL
      lockIndex   = 0UL
      key         = key
      flags       = defaultArg flags 0UL
      value       = value
      session     = None }

  static member Create(key : Key, value : string) =
    KVPair.Create(key, Encoding.UTF8.GetBytes value)

  static member inline CreateForAcquire(session : Session, key : Key, value : 'T, ?flags) =
    let json = Json.serialize value |> Json.format |> Encoding.UTF8.GetBytes
    { KVPair.Create(key, json, defaultArg flags 0UL) with
        session = Some session }

  static member FromJson (_ : KVPair) =
    (fun ci mi li k fl v s ->
      { createIndex = ci
        modifyIndex = mi
        lockIndex   = li
        key         = k
        flags       = fl
        value       = match v with
                      | None   -> [||]
                      | Some v -> Convert.FromBase64String v
        session     = s })
    <!> Json.read "CreateIndex"
    <*> Json.read "ModifyIndex"
    <*> Json.read "LockIndex"
    <*> Json.read "Key"
    <*> Json.read "Flags"
    <*> Json.read "Value"
    <*> Json.tryRead "Session"

  static member ToJson (kv : KVPair) =
    Json.write "CreateIndex" kv.createIndex
    *> Json.write "ModifyIndex" kv.modifyIndex
    *> Json.write "LockIndex" kv.lockIndex
    *> Json.write "Key" kv.key
    *> Json.write "Flags" kv.flags
    *> Json.write "Value" (Convert.ToBase64String kv.value)
    *> Json.write "Session" kv.session

type KVPairs = KVPair list
/// Allows you to pass a list that is built into a single Key (string) in the end.
type Keys = string list

type HealthCheck =
  { node        : string
    checkId     : Check  // either.... this is Check
    name        : string // OR this is Check (see type abbrev above)
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

type ServiceEntry =
  { node    : Node
    service : AgentService
    checks  : HealthCheck list }

type SessionBehaviour =
  /// Release; cause any locks that are held to be released.
  | Release
  /// Delete is useful for creating ephemeral key/value entries.
  | Delete

  static member FromJson (_ : SessionBehaviour) =
    (function
    | "release" -> Json.init Release
    | "delete " -> Json.init Delete
    | other     -> Json.error (sprintf "'%s' is not a valid session behaviour" other))
    =<< Json.Optic.get Json.String_

  static member ToJson (sb : SessionBehaviour) =
    Json.Optic.set Json.String_
                    (match sb with
                    | Release -> "release"
                    | Delete  -> "delete")

type SessionOption =
  | LockDelay of Duration
  | Node of string
  | Name of string
  | Checks of Check list
  | Behaviour of SessionBehaviour
  /// A **minumum** of 10 seconds! Otherwise you'll get 400 or 500 Bad Request back.
  | TTL of Duration

type SessionOptions = SessionOption list

/// SessionEntry represents a session in consul 
type SessionEntry =
    /// The epoch this session was created during. (You know if your session-based
    /// lock in the KV module is valid if the tuple of (Key, LockIndex, Session),
    /// which acts a unique "sequencer", matches the createIndex of this data
    /// structure. See https://www.consul.io/docs/internals/sessions.html (towards
    /// the bottom) for details.
  { createIndex : uint64
    /// The Guid/UUID of the session
    id          : Guid
    /// Name can be used to provide a human-readable name for the Session.
    name        : string
    /// Node must refer to a node that is already registered, if specified. By default, the agent's own node name is used.
    node        : string
    /// Checks is used to provide a list of associated health checks. It is highly recommended that, if you override this list, you include the default "serfHealth".
    checks      : string list
    /// LockDelay can be specified as a duration string using a "s" suffix for seconds. The default is 15s.
    lockDelay   : Duration
    /// Behavior can be set to either release or delete. This controls the behavior when a session is invalidated. By default, this is release, causing any locks that are held to be released. Changing this to delete causes any locks that are held to be deleted. delete is useful for creating ephemeral key/value entries.
    behavior    : string
    /// The TTL field is a duration string, and like LockDelay it can use "s" as a suffix for seconds. If specified, it must be between 10s and 3600s currently. When provided, the session is invalidated if it is not renewed before the TTL expires. See the session internals page for more documentation of this feature.
    ttl         : Duration }

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

type ReadConsistency =
  /// If not specified, the default is strongly consistent in almost all cases. However, there is a small window in which a new leader may be elected during which the old leader may service stale values. The trade-off is fast reads but potentially stale values. The condition resulting in stale reads is hard to trigger, and most clients should not need to worry about this case. Also, note that this race condition only applies to reads, not writes.
  | Default
  /// This mode is strongly consistent without caveats. It requires that a leader verify with a quorum of peers that it is still leader. This introduces an additional round-trip to all server nodes. The trade-off is increased latency due to an extra round trip. Most clients should not use this unless they cannot tolerate a stale read.
  | Consistent
  /// This mode allows any server to service the read regardless of whether it is the leader. This means reads can be arbitrarily stale; however, results are generally consistent to within 50 milliseconds of the leader. The trade-off is very fast and scalable reads with a higher likelihood of stale values. Since this mode allows reads without a leader, a cluster that is unavailable will still be able to respond to queries.
  | Stale

type QueryOption =
  | ReadConsistency of ReadConsistency
  /// WaitIndex is used to enable a blocking query. Waits
  /// until the timeout or the next index is reached
  /// WaitTime is used to bound the duration of a wait.
  /// Defaults to that of the Config, but can be overriden.
  | Wait of Index * Duration
  /// Token is used to provide a per-request ACL token
  /// which overrides the agent's default token.
  | TokenOverride of Token
  /// Providing a datacenter overwrites the DC provided
  /// by the Config
  | DataCenter of string

type QueryOptions = QueryOption list

type WriteMeta =
  { requestTime : Duration }

type WriteOption =
  | DataCenter of string
  | TokenOverride of Token

type WriteOptions = WriteOption list

type Error =
  | Message of string
  | KeyNotFound of Key
  | ConnectionFailed of System.Net.WebException

type FaktaConfig =
    /// The base URIs that we have servers at
  { serverBaseUri  : Uri
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
    { serverBaseUri  = Uri "http://127.0.0.1:8500"
      datacenter     = None
      credentials    = None
      waitTime       = DefaultLockWaitTime
      token          = None }

type FaktaState =
  { config : FaktaConfig
    logger : Logger
    clock  : IClock
    random : Random }

  static member Default =
    { config = FaktaConfig.Default
      logger = NoopLogger
      clock  = SystemClock.Instance
      random = Random () }