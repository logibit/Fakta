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

type CheckUpdate =
  {
    status : string
    output : string
  }

  static member GetUpdateJson (st : string)(out : string) =
    let res = {
                  status = st;
                  output = out
              }
    res

  static member ToJson (chu : CheckUpdate) =
    Json.write "Status" chu.status
    *> Json.write "Output" chu.output

type Node =
  { node    : string
    address : string }

  static member emptyNode =
    let en = {
                node = String.Empty;
                address = String.Empty
             }
    en

  static member FromJson (_ : Node) =
      (fun n a ->
        { node = n
          address = a
            })
      <!> Json.read "Node"
      <*> Json.read "Address"

  static member ToJson (n : Node) =
    Json.write "Node" n.node
    *> Json.write "Address" n.address

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
  static member FromJson (_ : AgentCheck) =
      (fun nd chId n st ns out sId sName ->
        { node = nd
          checkID = chId
          name = n
          status = st
          notes = ns
          output = out
          serviceId = sId
          serviceName = sName
            })
      <!> Json.read "Node"
      <*> Json.read "CheckID"
      <*> Json.read "Name"
      <*> Json.read "Status"
      <*> Json.read "Notes"
      <*> Json.read "Output"
      <*> Json.read "ServiceID"
      <*> Json.read "ServiceName"

  static member ToJson (ac : AgentCheck) =
    Json.write "Node" ac.node
    *> Json.write "CheckID" ac.checkID
    *> Json.write "Name" ac.name
    *> Json.write "Status" ac.status
    *> Json.write "Notes" ac.notes
    *> Json.write "Output" ac.output
    *> Json.write "ServiceID" ac.serviceId
    *> Json.write "ServiceName" ac.serviceName

type AgentMember =
  { name        : string
    addr        : string
    port        : uint16
    tags        : Map<string, string>
    status      : int
    protocolMin : int
    protocolMax : int
    protocolCur : int
    delegateMin : int
    delegateMax : int
    delegateCur : int }

  static member FromJson (_ : AgentMember) =
    (fun n addr p ts st pMin pMax pCur dMin dMax dCur ->
      { name = n
        addr = addr
        port   = p
        tags = ts
        status   = st
        protocolMin = pMin
        protocolMax = pMax
        protocolCur = pCur
        delegateMin = dMin
        delegateMax = dMax
        delegateCur = dCur
          })
    <!> Json.read "Name"
    <*> Json.read "Addr"
    <*> Json.read "Port"
    <*> Json.read "Tags"
    <*> Json.read "Status"
    <*> Json.read "ProtocolMin"
    <*> Json.read "ProtocolMax"
    <*> Json.read "ProtocolCur"
    <*> Json.read "DelegateMin"
    <*> Json.read "DelegateMax"
    <*> Json.read "DelegateCur"
  
  static member ToJson (am : AgentMember) =
    Json.write "Name" am.name
    *> Json.write "Addr" am.addr
    *> Json.write "Tags" am.tags
    *> Json.write "Port" am.port
    *> Json.write "Status" am.status
    *> Json.write "ProtocolMin" am.protocolMin
    *> Json.write "ProtocolMax" am.protocolMax
    *> Json.write "ProtocolCur" am.protocolCur
    *> Json.write "DelegateMin" am.delegateMin
    *> Json.write "DelegateMax" am.delegateMax
    *> Json.write "DelegateCur" am.delegateCur

type AgentService =
  { id                : Id
    service           : string
    tags              : string list
    port              : Port
    address           : string
    enableTagOverride : bool 
    createIndex       : int
    modifyIndex       : int}

  static member FromJson (_ : AgentService) =
    (fun id s t p a eto ci mi ->
      { id = id
        service = s
        tags   = t
        port = p
        address   = a
        enableTagOverride = eto
        createIndex = ci
        modifyIndex = mi
          })
    <!> Json.read "ID"
    <*> Json.read "Service"
    <*> Json.read "Tags"
    <*> Json.read "Port"
    <*> Json.read "Address"
    <*> Json.read "EnableTagOverride"
    <*> Json.read "CreateIndex"
    <*> Json.read "ModifyIndex"
  
  static member ToJson (ags : AgentService) =
    Json.write "ID" ags.id
    *> Json.write "Service" ags.service
    *> Json.write "Tags" ags.tags
    *> Json.write "Port" ags.port
    *> Json.write "Address" ags.address
    *> Json.write "EnableTagOverride" ags.enableTagOverride
    *> Json.write "CreateIndex" ags.createIndex
    *> Json.write "ModifyIndex" ags.modifyIndex

  

type AgentServiceCheck =
  {    
    script   : string option// `json:",omitempty"`
    interval : string option// `json:",omitempty"`
    timeout  : string option// `json:",omitempty"`
    ttl      : string option// `json:",omitempty"`
    http     : string option// `json:",omitempty"`
    tcp      : string option
    docker_container_id :string option
    shell    : string option
    status   : string option  } // `json:",omitempty"`

  static member ttlServiceCheck =
    let res = {
                  script = None
                  interval = None
                  timeout = None
                  ttl = Some("30s")
                  http = None
                  tcp = None
                  docker_container_id = None
                  shell = None
                  status = None
              }
    res

  static member FromJson (_ : AgentServiceCheck) =
    (fun sc i t ttl http tcp dci sh st ->
      { 
        script = sc
        interval = i
        timeout = t
        ttl = ttl
        http = http
        tcp = tcp
        docker_container_id = dci
        shell = sh
        status = st
          })
    <!> Json.read "script"
    <*> Json.read "interval"
    <*> Json.read "timeout"
    <*> Json.read "ttl"
    <*> Json.read "http"
    <*> Json.read "tcp"
    <*> Json.read "docker_container_id"
    <*> Json.read "shell"
    <*> Json.read "status"
  
  static member ToJson (ags : AgentServiceCheck) =
    Json.write "script" ags.script
    *> Json.write "interval" ags.interval
    *> Json.write "timeout" ags.timeout
    *> Json.write "ttl" ags.ttl
    *> Json.write "http" ags.http
    *> Json.write "tcp" ags.http
    *> Json.write "docker_container_id" ags.http
    *> Json.write "shell" ags.http
    *> Json.write "status" ags.status


type AgentServiceChecks = AgentServiceCheck list

type AgentServiceRegistration =
  { id      : string //   `json:",omitempty"`
    Name    : string //  `json:",omitempty"`
    Tags    : string list // `json:",omitempty"`
    Port    : int //     `json:",omitempty"`
    Address : string //  `json:",omitempty"`
    enableTagOverride : bool
    Check   : AgentServiceCheck
    checks  : AgentServiceChecks }

    static member serviceRegistration : (AgentServiceRegistration) =
      let res = {
                    id = "serviceReg123"; 
                    Name="serviceReg"; 
                    Tags = []; 
                    Address="127.0.0.1"; 
                    Port=8500; 
                    enableTagOverride = false; 
                    Check = AgentServiceCheck.ttlServiceCheck;
                    checks = []
                }
      res

    static member FromJson (_ : AgentServiceRegistration) =
    (fun id n ts p a eto ch chs ->
      { 
        id = id
        Name = n
        Tags = ts
        Port = p
        Address = a
        enableTagOverride = eto
        Check = ch
        checks = chs
          })
    <!> Json.read "id"
    <*> Json.read "Name"
    <*> Json.read "Tags"
    <*> Json.read "Port"
    <*> Json.read "Address"
    <*> Json.read "enableTagOverride"
    <*> Json.read "Check"
    <*> Json.read "checks"
  
  static member ToJson (ags : AgentServiceRegistration) =
    Json.write "id" ags.id
    *> Json.write "Name" ags.Name
    *> Json.write "Tags" ags.Tags
    *> Json.write "Port" ags.Port
    *> Json.write "Address" ags.Address
    *> Json.write "enableTagOverride" ags.enableTagOverride
    *> Json.write "Check" ags.Check
    *> Json.write "checks" ags.checks

type AgentCheckRegistration =
  { id        : Id option // `json:",omitempty"`
    name      : string option//`json:",omitempty"`
    notes     : string option// `json:",omitempty"`
    serviceId : Id option// `json:",omitempty"`
    check     : AgentServiceCheck }

  static member ttlCheck : (AgentCheckRegistration) =
    let res = { id = Some("consul"); name = Some("web app"); notes = None; serviceId = Some("consul");check = AgentServiceCheck.ttlServiceCheck;}
    res  
     

  static member FromJson (_ : AgentCheckRegistration) =
    (fun id s t p a eto ci mi ->
      { id = id
        name = s
        notes   = t
        serviceId = p
        check   = a
          })
    <!> Json.read "id"
    <*> Json.read "name"
    <*> Json.read "notes"
    <*> Json.read "service_id"
    <*> Json.read "check"
  
  static member ToJson (ags : AgentCheckRegistration) =
    Json.write "id" ags.id
    *> Json.write "name" ags.name
    *> Json.write "notes" ags.notes
    *> Json.write "service_id" ags.serviceId
    *> Json.write "check" ags.check

type CatalogDeregistration =
  { node       : string
    address    : string
    datacenter : string
    serviceId  : string
    checkId    : string }

type CatalogNode =
  { node     : Node
    Services : Map<string, AgentService> }

  static member emptyCatalogNode =
    let ec = {
                  Services = Map.empty
                  node = Node.emptyNode
             }
    ec

  static member FromJson (_ : CatalogNode) =
      (fun n s ->
        { node = n
          Services = s
            })
      <!> Json.read "Node"
      <*> Json.read "Services"

  static member ToJson (n : CatalogNode) =
    Json.write "Node" n.node
    *> Json.write "Services" n.Services

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

  static member FromJson (_ : HealthCheck) =
    (fun node id name status notes out serviceId serviceName ->
      { node = node
        checkId = id
        name   = name
        status = status 
        notes         = notes
        output       = out
        serviceId      = serviceId
        serviceName     = serviceName })
    <!> Json.read "Node"
    <*> Json.read "CheckID"
    <*> Json.read "Name"
    <*> Json.read "Status"
    <*> Json.read "Notes"
    <*> Json.read "Output"
    <*> Json.read "ServiceID"
    <*> Json.read "ServiceName"

  static member ToJson (hc : HealthCheck) =
    Json.write "Node" hc.node
    *> Json.write "CheckID" hc.checkId
    *> Json.write "Name" hc.name
    *> Json.write "Status" hc.status
    *> Json.write "Notes" hc.notes
    *> Json.write "Output" hc.output
    *> Json.write "ServiceID" hc.serviceId
    *> Json.write "ServiceName" hc.serviceName


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
    checks  : HealthCheck list}

  static member FromJson (_ : ServiceEntry) =
    (fun n s chs ->
      { node = n
        service = s
        checks   = chs
          })
    <!> Json.read "Node"
    <*> Json.read "Service"
    <*> Json.read "Checks"

  static member ToJson (se : ServiceEntry) =
    Json.write "Node" se.node
    *> Json.write "Service" se.service
    *> Json.write "Checks" se.checks

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