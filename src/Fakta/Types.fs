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

/// Tells the programmer this feature has not been implemented yet.
exception TBDException of reason:string


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
  { status : string
    output : string}

  static member GetUpdateJson (st: string) (out: string) =
    { status = st
      output = out }

  static member ToJson (chu: CheckUpdate) =
    Json.write "Status" chu.status
    *> Json.write "Output" chu.output

type Node =
  { node    : string
    address : string }

  static member FromJson (_: Node) =
    (fun n a ->
      { node = n
        address = a })
    <!> Json.read "Node"
    <*> Json.read "Address"

  static member ToJson (n: Node) =
    Json.write "Node" n.node
    *> Json.write "Address" n.address

type ACLEntry =
  { createIndex : Index
    modifyIndex : Index
    id          : Id
    name        : string
    ``type``    : string
    rules       : string }

  static member ClientTokenInstance (tokenID: Id) (tokenName: string) (tokenType: string) =
    { createIndex = Index.MinValue
      modifyIndex = Index.MinValue
      id = tokenID
      name = tokenName
      ``type`` = tokenType
      rules = "" }

  static member empty =
    { createIndex = Index.MinValue
      modifyIndex = Index.MinValue
      id = ""
      name = ""
      ``type`` = ""
      rules = "" }

  static member FromJson (_: ACLEntry) =
    (fun ci mi i n t rs ->
      { createIndex = ci
        modifyIndex = mi
        id          = i
        name        = n
        ``type``    = t
        rules       = rs })
    <!> Json.read "CreateIndex"
    <*> Json.read "ModifyIndex"
    <*> Json.read "ID"
    <*> Json.read "Name"
    <*> Json.read "Type"
    <*> Json.read "Rules"

  static member ToJson (ac: ACLEntry) =
    Json.write "CreateIndex" ac.createIndex
    *> Json.write "ModifyIndex" ac.modifyIndex
    *> Json.write "ID" ac.id
    *> Json.write "Name" ac.name
    *> Json.write "Type" ac.``type``
    *> Json.write "Rules" ac.rules

type AgentCheck =
  { node        : string
    checkID     : string
    name        : string
    status      : string
    notes       : string
    output      : string
    serviceId   : string
    serviceName : string }



  static member FromJson (_: AgentCheck) =
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

  static member ToJson (ac: AgentCheck) =
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

  static member FromJson (_: AgentMember) =
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
        delegateCur = dCur })
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

  static member ToJson (am: AgentMember) =
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
    tags              : string list option
    port              : Port
    address           : string
    enableTagOverride : bool
    createIndex       : int
    modifyIndex       : int}


  static member FromJson (_: AgentService) =
    (fun id s t p a eto ci mi ->
      { id = id
        service = s
        tags   = match t with
                 | Some a -> a
                 | None -> Some([])
        port = p
        address   = a
        enableTagOverride = eto
        createIndex = ci
        modifyIndex = mi })
    <!> Json.read "ID"
    <*> Json.read "Service"
    <*> Json.read "Tags"
    <*> Json.read "Port"
    <*> Json.read "Address"
    <*> Json.read "EnableTagOverride"
    <*> Json.read "CreateIndex"
    <*> Json.read "ModifyIndex"

  static member ToJson (ags: AgentService) =
    //let tags = if ags.tags = null then [] else args.tags
    Json.write "ID" ags.id
    *> Json.write "Service" ags.service
    *> Json.write "Tags" ags.tags
    *> Json.write "Port" ags.port
    *> Json.write "Address" ags.address
    *> Json.write "EnableTagOverride" ags.enableTagOverride
    *> Json.write "CreateIndex" ags.createIndex
    *> Json.write "ModifyIndex" ags.modifyIndex

type AgentServiceCheck =
  { script   : string option// `json:",omitempty"`
    interval : string option// `json:",omitempty"`
    timeout  : string option// `json:",omitempty"`
    ttl      : string option// `json:",omitempty"`
    http     : string option// `json:",omitempty"`
    tcp      : string
    dockerContainerId :string
    shell    : string
    status   : string option  } // `json:",omitempty"`

  static member ttlServiceCheck =
    { script = Some("")
      interval = Some("15s")
      timeout = Some("")
      ttl = Some("30s")
      http = Some("")
      tcp = ""
      dockerContainerId = ""
      shell = ""
      status = Some("") }

  static member FromJson (_: AgentServiceCheck) =
    (fun sc i t ttl http tcp dci sh st ->
      { script = sc
        interval = i
        timeout = t
        ttl = ttl
        http = http
        tcp = tcp
        dockerContainerId = dci
        shell = sh
        status = st })
    <!> Json.read "Script"
    <*> Json.read "Interval"
    <*> Json.read "Timeout"
    <*> Json.read "TTL"
    <*> Json.read "HTTP"
    <*> Json.read "TCP"
    <*> Json.read "DockerContainerID"
    <*> Json.read "Shell"
    <*> Json.read "Status"

  static member ToJson (ags: AgentServiceCheck) =
    Json.write "Script" ags.script
    *> Json.maybeWrite "Interval" ags.interval
    *> Json.maybeWrite "Timeout" ags.timeout
    *> Json.maybeWrite "TTL" ags.ttl
    *> Json.maybeWrite "HTTP" ags.http
    *> Json.write "TCP" ags.tcp
    *> Json.write "DockerContainerID" ags.dockerContainerId
    *> Json.write "Shell" ags.shell
    *> Json.maybeWrite "Status" ags.status


type AgentServiceChecks = AgentServiceCheck list

type AgentServiceRegistration =
  { id      : string option //   `json:",omitempty"`
    name    : string option //  `json:",omitempty"`
    tags    : string list option // `json:",omitempty"`
    port    : int option //     `json:",omitempty"`
    address : string option //  `json:",omitempty"`
    enableTagOverride : bool
    check   : AgentServiceCheck option
    checks  : AgentServiceChecks option }

    static member serviceRegistration (id: string) : (AgentServiceRegistration) =
      { id = Some(id)
        name= Some("serviceReg")
        tags = None
        address = Some("127.0.0.1")
        port = Some 8500
        enableTagOverride = false
        check = None
        checks = None }

    static member FromJson (_: AgentServiceRegistration) =
      (fun id n ts p a eto ch chs ->
        { id = id
          name = n
          tags = ts
          port = p
          address = a
          enableTagOverride = eto
          check = ch
          checks = chs })
      <!> Json.read "ID"
      <*> Json.read "Name"
      <*> Json.read "Tags"
      <*> Json.read "Port"
      <*> Json.read "Address"
      <*> Json.read "EnableTagOverride"
      <*> Json.read "Check"
      <*> Json.read "Checks"

  static member ToJson (ags: AgentServiceRegistration) =
    Json.write "ID" ags.id
    *> Json.maybeWrite "Name" ags.name
    *> Json.maybeWrite "Tags" ags.tags
    *> Json.maybeWrite "Port" ags.port
    *> Json.maybeWrite "Address" ags.address
    *> Json.write "EnableTagOverride" ags.enableTagOverride
    *> Json.maybeWrite "Check" ags.check
    *> Json.maybeWrite "Checks" ags.checks

type AgentCheckRegistration =
    /// If an ID is not provided, it is set to Name. You cannot have duplicate
    /// ID entries per agent, so it may be necessary to provide an ID.
  { id        : Id option // `json:",omitempty"`
    /// The Name field is mandatory, as is one of Script, HTTP, TCP or TTL.
    /// Script, TCP and HTTP also require that Interval be set.
    name      : string//`json:",omitempty"`
    /// The Notes field is not used internally by Consul and is meant to be human-readable.
    notes     : string option// `json:",omitempty"`
    /// The ServiceID field can be provided to associate the registered check
    /// with an existing service provided by the agent.
    serviceId : Id option// `json:",omitempty"`
    /// If a Script is provided, the check type is a script, and Consul will evaluate the script every Interval to update the status.
    script   : string option// `json:",omitempty"`
    interval : string option// `json:",omitempty"`
    timeout  : string option// `json:",omitempty"`
    /// If a TTL type is used, then the TTL update endpoint must be used
    /// periodically to update the state of the check.
    ttl      : string option// `json:",omitempty"`
    /// An HTTP check will perform an HTTP GET request against the value of HTTP
    /// (expected to be a URL) every Interval. If the response is any 2xx code,
    /// the check is passing. If the response is 429 Too Many Requests, the
    /// check is warning. Otherwise, the check is critical.
    http     : string option// `json:",omitempty"`
    /// An TCP check will perform an TCP connection attempt against the value of
    /// TCP (expected to be an IP/hostname and port combination) every Interval.
    /// If the connection attempt is successful, the check is passing. If the
    /// connection attempt is unsuccessful, the check is critical. In the case
    /// of a hostname that resolves to both IPv4 and IPv6 addresses, an attempt
    /// will be made to both addresses, and the first successful connection
    /// attempt will result in a successful check.
    tcp      : string option
    /// If a DockerContainerID is provided, the check is a Docker check, and
    /// Consul will evaluate the script every Interval in the given container
    /// using the specified Shell. Note that Shell is currently only supported
    /// for Docker checks.
    dockerContainerId :string option
    shell    : string option
    /// The Status field can be provided to specify the initial state of the
    /// health check.
    status   : string option }

  static member ttlCheck (id: string) (name: string) (serviceId: string) (intr: string) (ttl: string) : (AgentCheckRegistration) =    
      { id = Some(id); 
        name = name; 
        notes = None;
        serviceId =Some(serviceId);
        script = None
        interval = Some(intr)
        timeout = None
        ttl = Some(ttl)
        http = None
        tcp = None
        dockerContainerId = None
        shell = None
        status = None }

  static member FromJson (_: AgentCheckRegistration) =
    (fun id n nt si sc i t ttl http tcp dci sh st ->
      { id = id
        name = n
        notes   = nt
        serviceId = si
        script = sc
        interval = i
        timeout = t
        ttl = ttl
        http = http
        tcp = tcp
        dockerContainerId = dci
        shell = sh
        status = st })
    <!> Json.read "ID"
    <*> Json.read "Name"
    <*> Json.read "Notes"
    <*> Json.read "ServiceID"
    <*> Json.read "Script"
    <*> Json.read "Interval"
    <*> Json.read "Timeout"
    <*> Json.read "TTL"
    <*> Json.read "HTTP"
    <*> Json.read "TCP"
    <*> Json.read "DockerContainerID"
    <*> Json.read "Shell"
    <*> Json.read "Status"

  static member ToJson (ags: AgentCheckRegistration) =
    Json.maybeWrite "ID" ags.id
    *> Json.write "Name" ags.name
    *> Json.maybeWrite "Notes" ags.notes //Json.write "Notes" ags.notes
    *> Json.maybeWrite "ServiceID" ags.serviceId
    *> Json.maybeWrite "ID" ags.id
    *> Json.maybeWrite "Script" ags.script
    *> Json.maybeWrite "Interval" ags.interval
    *> Json.maybeWrite "Timeout" ags.timeout
    *> Json.maybeWrite "TTL" ags.ttl
    *> Json.maybeWrite "HTTP" ags.http
    *> Json.maybeWrite "TCP" ags.tcp
    *> Json.maybeWrite "DockerContainerID" ags.dockerContainerId
    *> Json.maybeWrite "Shell" ags.shell
    *> Json.maybeWrite "Status" ags.status

type CatalogDeregistration =
  { node       : string
    address    : string
    datacenter : string
    serviceId  : string
    checkId    : string }

    static member Instance (nodeName: string) (dc: string) (addr: string) (serviceId: string) (checkID: string) =
        { node = nodeName
          datacenter = dc
          address = addr
          serviceId = serviceId
          checkId = checkID }

  static member FromJson (_: CatalogDeregistration) =
    (fun n a dc si chi ->
      { node = n
        address = a
        datacenter   = dc
        serviceId = si
        checkId   = chi
          })
    <!> Json.read "Node"
    <*> Json.read "Address"
    <*> Json.read "Datacenter"
    <*> Json.read "ServiceID"
    <*> Json.read "CheckID"

  static member ToJson (cdr: CatalogDeregistration) =
    Json.write "Node" cdr.node
    *> Json.write "Address" cdr.address
    *> Json.write "Datacenter" cdr.datacenter
    *> Json.write "ServiceID" cdr.serviceId
    *> Json.write "CheckID" cdr.checkId


type CatalogNode =
  { node     : Node
    services : Map<string, AgentService> }

  static member FromJson (_: CatalogNode) =
    (fun n s ->
      { node = n
        services = s })
    <!> Json.read "Node"
    <*> Json.read "Services"

  static member ToJson (n: CatalogNode) =
    Json.write "Node" n.node
    *> Json.write "Services" n.services

type CatalogRegistration =
  { node       : string
    address    : string
    datacenter : string
    service    : AgentService
    check      : AgentCheck }

  static member Instance (nodeName: string) (address: string) (dc: string) (agentCheck: AgentCheck) (agentServicë: AgentService) =
    {   node = nodeName
        address = address        
        service = agentServicë
        datacenter = dc
        check = agentCheck }

  static member FromJson (_: CatalogRegistration) =
    (fun n a dc s ch ->
      { node = n
        address = a
        datacenter = dc
        service = s
        check = ch })
    <!> Json.read "Node"
    <*> Json.read "Address"
    <*> Json.read "Datacenter"
    <*> Json.read "Service"
    <*> Json.read "Check"

  static member ToJson (n: CatalogRegistration) =
    Json.write "Node" n.node
    *> Json.write "Address" n.address
    *> Json.write "Datacenter" n.datacenter
    *> Json.write "Service" n.service
    *> Json.write "Check" n.check

type CatalogService =
  { node           : string
    address        : string
    serviceID      : string
    serviceName    : string
    serviceAddress : string
    serviceTags    : string list
    servicePort    : int }

  static member FromJson (_: CatalogService) =
    (fun n a sid sn sa sts sp ->
      { node = n
        address = a
        serviceID = sid
        serviceName = sn
        serviceAddress = sa
        serviceTags = sts
        servicePort = sp })
    <!> Json.read "Node"
    <*> Json.read "Address"
    <*> Json.read "ServiceID"
    <*> Json.read "ServiceName"
    <*> Json.read "ServiceAddress"
    <*> Json.read "ServiceTags"
    <*> Json.read "ServicePort"

  static member ToJson (s: CatalogService) =
    Json.write "Node" s.node
    *> Json.write "Address" s.address
    *> Json.write "ServiceID" s.serviceID
    *> Json.write "ServiceName" s.serviceName
    *> Json.write "ServiceAddress" s.serviceAddress
    *> Json.write "ServiceTags" s.serviceTags
    *> Json.write "ServicePort" s.servicePort

type PortMapping =
  { dns : Port
    http : Port
    rpc : Port
    serfLan : Port
    serfWan : Port
    server : Port }

  static member FromJson (_: PortMapping) =
    (fun d h r slan swan s ->
      { dns = d
        http = h
        rpc = r
        serfLan = slan
        serfWan = swan
        server = s })
    <!> Json.read "DNS"
    <*> Json.read "HTTP"
    <*> Json.read "RPC"
    <*> Json.read "SerfLan"
    <*> Json.read "SerfWan"
    <*> Json.read "Server"

  static member ToJson (pm: PortMapping) =
    Json.write "DNS" pm.dns
    *> Json.write "HTTP" pm.http
    *> Json.write "RPC" pm.rpc
    *> Json.write "SerfLan" pm.serfLan
    *> Json.write "SerfWan" pm.serfWan
    *> Json.write "Server" pm.server

type ConfigData =
  { boostrap : bool
    server   : bool
    datacenter : string
    dataDir    : string
    dnsRecursor : string
    dnsRecursors : string list
    domain : string
    logLevel : string
    nodeName : string
    clientAddr : string
    bindAddr : string
    advertiseAddr : string
    ports : PortMapping
    leaveOnTerm : bool
    skipLeaveOnInt : bool
    //statsiteAddr : string
    protocol : uint16
    enableDebug : bool
    verifyIncoming : bool
    verifyOutgoing : bool
    caFile : string option
    certFile : string option
    keyFile : string option
    startJoin : string list
    uiDir : string option
    pidFile : string option
    enableSyslog : bool
    rejoinAfterLeave : bool }

  static member FromJson (_: ConfigData) =
    (fun boot srv dc ddir dnsrec dnsrecs d llvl node ca binda adva ps lot slot pr edeb vi vo caf certf kf stj udir pf esl ral ->
      { boostrap = boot
        server = srv
        datacenter = dc
        dataDir = ddir
        dnsRecursor = dnsrec
        dnsRecursors = dnsrecs
        domain = d
        logLevel = llvl
        nodeName = node
        clientAddr = ca
        bindAddr = binda
        advertiseAddr = adva
        ports = ps
        leaveOnTerm = lot
        skipLeaveOnInt = slot
        //statsiteAddr = statsa 
        protocol = pr
        enableDebug = edeb
        verifyIncoming = vi
        verifyOutgoing = vo
        caFile = caf
        certFile = certf
        keyFile = kf
        startJoin = stj
        uiDir = udir
        pidFile = pf
        enableSyslog = esl
        rejoinAfterLeave = ral})
    <!> Json.read "Bootstrap"
    <*> Json.read "Server"
    <*> Json.read "Datacenter"
    <*> Json.read "DataDir"
    <*> Json.read "DNSRecursor"
    <*> Json.read "DNSRecursors"
    <*> Json.read "Domain"
    <*> Json.read "LogLevel"
    <*> Json.read "NodeName"
    <*> Json.read "ClientAddr"
    <*> Json.read "BindAddr"
    <*> Json.read "AdvertiseAddr"
    <*> Json.read "Ports"
    <*> Json.read "LeaveOnTerm"
    <*> Json.read "SkipLeaveOnInt"
    //<*> Json.read "StatsiteAddr"
    <*> Json.read "Protocol"
    <*> Json.read "EnableDebug"
    <*> Json.read "VerifyIncoming"
    <*> Json.read "VerifyOutgoing"
    <*> Json.read "CAFile"
    <*> Json.read "CertFile"
    <*> Json.read "KeyFile"
    <*> Json.read "StartJoin"
    <*> Json.read "UiDir"
    <*> Json.read "PidFile"
    <*> Json.read "EnableSyslog"
    <*> Json.read "RejoinAfterLeave"

  static member ToJson (cd: ConfigData) =
    Json.write "Bootstrap" cd.boostrap
    *> Json.write "Server" cd.server
    *> Json.write "Datacenter" cd.datacenter
    *> Json.write "DataDir" cd.dataDir
    *> Json.write "DNSRecursor" cd.dnsRecursor
    *> Json.write "DNSRecursors" cd.dnsRecursors
    *> Json.write "Domain" cd.domain
    *> Json.write "LogLevel" cd.logLevel
    *> Json.write "NodeName" cd.nodeName
    *> Json.write "ClientAddr" cd.clientAddr
    *> Json.write "BindAddr" cd.bindAddr
    *> Json.write "AdvertiseAddr" cd.advertiseAddr
    *> Json.write "Ports" cd.ports
    *> Json.write "LeaveOnTerm" cd.leaveOnTerm
    *> Json.write "SkipLeaveOnInt" cd.skipLeaveOnInt
    //*> Json.write "StatsiteAddr" cd.statsiteAddr
    *> Json.write "Protocol" cd.protocol
    *> Json.write "EnableDebug" cd.enableDebug
    *> Json.write "VerifyIncoming" cd.verifyIncoming
    *> Json.write "VerifyOutgoing" cd.verifyOutgoing
    *> Json.write "CAFile" cd.caFile
    *> Json.write "CertFile" cd.certFile
    *> Json.write "KeyFile" cd.keyFile
    *> Json.write "StartJoin" cd.startJoin
    *> Json.write "UiDir" cd.uiDir
    *> Json.write "PidFile" cd.pidFile
    *> Json.write "EnableSyslog" cd.enableSyslog
    *> Json.write "RejoinAfterLeave" cd.rejoinAfterLeave

type CoordData =
  { adjustment : int
    error : float
    vec : int list }

  static member FromJson (_: CoordData) =
    (fun a e v ->
      { adjustment = a
        error = e
        vec = v})
    <!> Json.read "Adjustment"
    <*> Json.read "Error"
    <*> Json.read "Vec"
   

  static member ToJson (cd: CoordData) =
    Json.write "Adjustment" cd.adjustment
    *> Json.write "Error" cd.error
    *> Json.write "Vec" cd.vec

type MemberData =
  { name : string
    addr : string
    port : Port
    tags : Map<string, string>
    status : int
    protocolMin : uint16
    protocolMax : uint16
    protocolCur : uint16
    delegateMin : uint16
    delegateMax : uint16
    delegateCur : uint16 }

    static member FromJson (_: MemberData) =
      (fun n add p ts st pmin pmax pcur dmin dmax dcur ->
        { name = n
          addr = add
          port = p
          tags = ts
          status = st
          protocolMin = pmin 
          protocolMax = pmax
          protocolCur = pcur
          delegateMin = dmin
          delegateMax = dmax
          delegateCur = dcur})
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

  static member ToJson (md: MemberData) =
    Json.write "Name" md.name
    *> Json.write "Addr" md.addr
    *> Json.write "Port" md.port
    *> Json.write "Tags" md.tags
    *> Json.write "Status" md.status
    *> Json.write "ProtocolMin" md.protocolMin
    *> Json.write "ProtocolMax" md.protocolMax
    *> Json.write "ProtocolCur" md.protocolCur
    *> Json.write "DelegateMin" md.delegateMin
    *> Json.write "DelegateMax" md.delegateMax
    *> Json.write "DelegateCur" md.delegateCur

type SelfData =
  { config : ConfigData
    coord  : CoordData
    ``member`` : MemberData }

  static member FromJson (_: SelfData) =
    (fun conf coord mem ->
      { config = conf
        coord = coord
        ``member`` = mem })
    <!> Json.read "Config"
    <*> Json.read "Coord"
    <*> Json.read "Member"

  static member ToJson (s: SelfData) =
    Json.write "Config" s.config
    *> Json.write "Coord" s.coord
    *> Json.write "Member" s.``member``



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

  static member create(key : Key, value : byte [], ?flags) =
    // TODO: validate that value <= 512 KiB
    { createIndex = 0UL
      modifyIndex = 0UL
      lockIndex   = 0UL
      key         = key
      flags       = defaultArg flags 0UL
      value       = value
      session     = None }

  static member Create(key : Key, value : string) =
    KVPair.create(key, UTF8.bytes value)

  static member inline CreateForAcquire(session : Session, key : Key, value : 'T, ?flags) =
    let json = Json.serialize value |> Json.format |> Encoding.UTF8.GetBytes
    { KVPair.create(key, json, defaultArg flags 0UL) with
        session = Some session }

  static member FromJson (_: KVPair) =
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

  static member ToJson (kv: KVPair) =
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

  static member FromJson (_: HealthCheck) =
    (fun node id name status notes out serviceId serviceName ->
      { node        = node
        checkId     = id
        name        = name
        status      = status
        notes       = notes
        output      = out
        serviceId   = serviceId
        serviceName = serviceName })
    <!> Json.read "Node"
    <*> Json.read "CheckID"
    <*> Json.read "Name"
    <*> Json.read "Status"
    <*> Json.read "Notes"
    <*> Json.read "Output"
    <*> Json.read "ServiceID"
    <*> Json.read "ServiceName"

  static member ToJson (hc: HealthCheck) =
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

  static member FromJson (_: ServiceEntry) =
    (fun n s chs ->
      { node    = n
        service = s
        checks  = chs })
    <!> Json.read "Node"
    <*> Json.read "Service"
    <*> Json.read "Checks"

  static member ToJson (se: ServiceEntry) =
    Json.write "Node" se.node
    *> Json.write "Service" se.service
    *> Json.write "Checks" se.checks

type SessionBehaviour =
  /// Release; cause any locks that are held to be released.
  | Release
  /// Delete is useful for creating ephemeral key/value entries.
  | Delete

  static member FromJson (_: SessionBehaviour) =
    (function
    | "release" -> Json.init Release
    | "delete " -> Json.init Delete
    | other     -> Json.error (sprintf "'%s' is not a valid session behaviour" other))
    =<< Json.Optic.get Json.String_

  static member ToJson (sb: SessionBehaviour) =
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
    lockDelay   : uint64
    /// Behavior can be set to either release or delete. This controls the behavior when a session is invalidated. By default, this is release, causing any locks that are held to be released. Changing this to delete causes any locks that are held to be deleted. delete is useful for creating ephemeral key/value entries.
    behavior    : string
    /// The TTL field is a duration string, and like LockDelay it can use "s" as a suffix for seconds. If specified, it must be between 10s and 3600s currently. When provided, the session is invalidated if it is not renewed before the TTL expires. See the session internals page for more documentation of this feature.
    ttl         : Duration }

  static member empty =
    { createIndex = UInt64.MinValue
      id = Guid.Empty
      name = ""
      node = ""
      checks = []
      lockDelay = UInt64.MinValue
      behavior = ""
      ttl = Duration.Epsilon }

  static member FromJson (_: SessionEntry) =
    (fun ci id n nd chs ld b ttl ->
      { createIndex = ci
        id = id
        name = n
        node = nd
        checks = chs
        lockDelay   = ld
        behavior = b
        ttl = ttl })
    <!> Json.read "CreateIndex"
    <*> Json.read "ID"
    <*> Json.read "Name"
    <*> Json.read "Node"
    <*> Json.read "Checks"
    <*> Json.read "LockDelay"
    <*> Json.read "Behavior"
    <*> Json.readWith Duration.FromJson "TTL"

  static member ToJson (se: SessionEntry) =
    Json.write "CreateIndex" se.createIndex
    *> Json.write "ID" se.id
    *> Json.write "Name" se.name
    *> Json.write "Node" se.node
    *> Json.write "Checks" se.checks
    *> Json.write "LockDelay" se.lockDelay
    *> Json.write "Behavior" se.behavior
    *> Duration.ToJson se.ttl

type UserEvent =
  { id            : Id
    name          : string
    payload       : byte []
    nodeFilter    : string
    serviceFilter : string
    tagFilter     : string
    version       : int
    lTime         : int }

  static member Instance (ID: string) (name: string) =
    { id = ID
      name = name
      payload = [||]
      nodeFilter = ""
      serviceFilter = ""
      tagFilter = ""
      version = 1
      lTime = 0 }

  static member empty =
    { id = ""
      name = ""
      payload = [||]
      nodeFilter = ""
      serviceFilter = ""
      tagFilter = ""
      version = -1
      lTime = -1 }

  static member FromJson (_: UserEvent) =
    (fun id n pl nf sf tf v lt ->
      { id = id
        name = n
        payload = match pl with
                  | None    -> [||]
                  | Some pl -> Convert.FromBase64String pl
        nodeFilter = nf
        serviceFilter = sf
        tagFilter     = tf
        version = v
        lTime = lt })
    <!> Json.read "ID"
    <*> Json.read "Name"
    <*> Json.read "Payload"
    <*> Json.read "NodeFilter"
    <*> Json.read "ServiceFilter"
    <*> Json.read "TagFilter"
    <*> Json.read "Version"
    <*> Json.read "LTime"

  static member ToJson (ue: UserEvent) =
    Json.write "ID" ue.id
    *> Json.write "Name" ue.name
    *> Json.write "Payload" (Convert.ToBase64String ue.payload)
    *> Json.write "NodeFilter" ue.nodeFilter
    *> Json.write "ServiceFilter" ue.serviceFilter
    *> Json.write "TagFilter" ue.tagFilter
    *> Json.write "Version" ue.version
    *> Json.write "LTime" ue.lTime

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
  | ResourceNotFound
  | KeyNotFound of Key
  | ConnectionFailed of System.Net.WebException

type FaktaConfig =
    /// The base URIs that we have servers at
  { serverBaseUri : Uri
    /// Datacenter to use. If not provided, the default agent datacenter is used.
    datacenter    : string option
    /// HttpAuth is the auth info to use for http access.
    credentials   : HttpBasicAuth option
    /// WaitTime limits how long a Watch will block. If not provided,
    /// the agent default values will be used.
    waitTime      : Duration
    /// Token is used to provide a per-request ACL token
    /// which overrides the agent's default token.
    token         : Token option
    keys          : Token list option }

  static member consulEmpty =
    { serverBaseUri  = Uri "http://127.0.0.1:8500"
      datacenter     = None
      credentials    = None
      waitTime       = DefaultLockWaitTime
      token          = None 
      keys           = None}

  static member vaultEmpty =
    { serverBaseUri  = Uri "http://127.0.0.1:8200"
      datacenter     = None
      credentials    = None
      waitTime       = DefaultLockWaitTime
      token          = None 
      keys           = None}

  static member vaultConfig (token: Token) (keys: Token list) =
    { serverBaseUri  = Uri "http://127.0.0.1:8200"
      datacenter     = None
      credentials    = None
      waitTime       = DefaultLockWaitTime
      token          = Some(token) 
      keys           = Some(keys)}

  

open System.Threading

let seedGenerator = new Random()
let random =
  new ThreadLocal<Random>(fun _ -> 
          lock seedGenerator (fun _ -> 
            let seed = seedGenerator.Next()
            new Random(seed)))

type APIType =
  | Consul
  | Vault

let private defaultLogger =
  Log.create "Fakta"

type FaktaState =
  { config      : FaktaConfig
    logger      : Logger
    clock       : IClock
    random      : Random
    clientState : HttpFs.Client.HttpFsState }

  static member emptyConsulConfig =
    { config      = FaktaConfig.consulEmpty
      logger      = defaultLogger
      clock       = SystemClock.Instance
      random      = random.Value
      clientState = HttpFs.Client.HttpFsState.empty }

  static member emptyVaultConfig =
    { config      = FaktaConfig.vaultEmpty
      logger      = defaultLogger 
      clock       = SystemClock.Instance
      random      = random.Value
      clientState = HttpFs.Client.HttpFsState.empty }

  static member create (api: APIType) (token: Token) (keys: Token list) (logger: Logger) (clientState : HttpFs.Client.HttpFsState) =
    { config = match api with 
               | APIType.Consul -> FaktaConfig.consulEmpty
               | APIType.Vault -> FaktaConfig.vaultConfig token keys
      logger      = logger
      clock       = SystemClock.Instance
      random      = random.Value
      clientState = clientState }

/// Vault types ///

type InitRequest =
  { secretShares: int
    secretThreshold: int
    pgpKeys : string list }

  static member FromJson (_: InitRequest) =
    (fun shs tr pgp ->
      { secretShares = shs
        secretThreshold = tr
        pgpKeys  = pgp })
    <!> Json.read "secret_shares"
    <*> Json.read "secret_threshold"
    <*> Json.read "pgp_keys"

  static member ToJson (se: InitRequest) =
    Json.write "secret_shares" se.secretShares
    *> Json.write "secret_threshold" se.secretThreshold
    *> Json.write "pgp_keys" se.pgpKeys

type InitResponse =
  { keys: string list
    recoveryKeys: string list option
    rootToken : string }

  static member FromJson (_: InitResponse) =
    (fun ks rcs rt ->
      { keys =ks
        recoveryKeys = rcs
        rootToken  = rt })
    <!> Json.read "keys"
    <*> Json.tryRead "recovery_keys"
    <*> Json.read "root_token"

  static member ToJson (se: InitResponse) =
    Json.write "keys" se.keys
    *> Json.maybeWrite "recovery_keys" se.recoveryKeys
    *> Json.write "root_token" se.rootToken


type SealStatusResponse =
  { ``sealed`` : bool
    t : int
    n : int
    progress : int}

  static member FromJson (_: SealStatusResponse) =
    (fun s t n p ->
      { ``sealed`` = s
        t = t
        n  = n
        progress  = p })
    <!> Json.read "sealed"
    <*> Json.read "t"
    <*> Json.read "n"
    <*> Json.read "progress"

  static member ToJson (se: SealStatusResponse) =
    Json.write "sealed" se.``sealed``
    *> Json.write "t" se.t
    *> Json.write "n" se.n
    *> Json.write "progress" se.progress

type GenerateRootStatusResponse =
  { nonce   : string
    started : bool
    progress: int
    required: int
    complete: bool
    encodedRootToken : string
    pgpFingerprint : string }

  static member FromJson (_: GenerateRootStatusResponse) =
    (fun nc s p r c er pgp ->
      { nonce = nc
        started = s
        progress = p
        required = r
        complete = c
        encodedRootToken = er
        pgpFingerprint = pgp
        })
    <!> Json.read "nonce"
    <*> Json.read "started"
    <*> Json.read "progress"
    <*> Json.read "required"
    <*> Json.read "complete"
    <*> Json.read "encoded_root_token"
    <*> Json.read "pgp_fingerprint"

  static member ToJson (se: GenerateRootStatusResponse) =
    Json.write "nonce" se.nonce
    *> Json.write "started" se.started
    *> Json.write "progress" se.progress
    *> Json.write "required" se.required
    *> Json.write "complete" se.complete
    *> Json.write "encoded_root_token" se.encodedRootToken
    *> Json.write "pgp_fingerprint" se.pgpFingerprint

type MountConfigInput =
  { defaultLeaseTTL : string
    maxLeaseTTL     : string}

  static member FromJson (_: MountConfigInput) =
    (fun d m ->
      { defaultLeaseTTL = d
        maxLeaseTTL = m})
    <!> Json.read "default_lease_ttl"
    <*> Json.read "max_lease_ttl"
    

  static member ToJson (se: MountConfigInput) =
    Json.write "default_lease_ttl" se.defaultLeaseTTL
    *> Json.write "max_lease_ttl" se.maxLeaseTTL

type MountConfigOutput =
  { defaultLeaseTTL : int
    maxLeaseTTL     : int}

  static member FromJson (_: MountConfigOutput) =
    (fun d m ->
      { defaultLeaseTTL = d
        maxLeaseTTL = m})
    <!> Json.read "default_lease_ttl"
    <*> Json.read "max_lease_ttl"
    

  static member ToJson (se: MountConfigOutput) =
    Json.write "default_lease_ttl" se.defaultLeaseTTL
    *> Json.write "max_lease_ttl" se.maxLeaseTTL

type MountOutput =
  { ``type``    : string
    description : string
    mConfig      : MountConfigOutput option}

  static member FromJson (_: MountOutput) =
    (fun t d c ->
      { ``type`` = t
        description = d
        mConfig = c})
    <!> Json.read "type"
    <*> Json.read "description"
    <*> Json.tryRead "config"
    

  static member ToJson (se: MountOutput) =
    Json.write "type" se.``type``
    *> Json.write "description" se.description
    *> Json.maybeWrite "config" se.mConfig

type MountInput =
  { ``type``    : string
    description : string
    mountConfig      : MountConfigInput option}

  static member FromJson (_: MountInput) =
    (fun t d c ->
      { ``type`` = t
        description = d
        mountConfig = c})
    <!> Json.read "type"
    <*> Json.read "description"
    <*> Json.tryRead "config"
    

  static member ToJson (se: MountInput) =
    Json.write "type" se.``type``
    *> Json.write "description" se.description
    *> Json.maybeWrite "config" se.mountConfig


type LeaderResponse =
  { haEnabled     : bool
    isSelf        : bool
    leaderAddress : string }

  static member FromJson (_: LeaderResponse) =
    (fun t d c ->
      { haEnabled = t
        isSelf = d
        leaderAddress = c})
    <!> Json.read "ha_enabled"
    <*> Json.read "is_self"
    <*> Json.read "leader_address"
    

  static member ToJson (se: LeaderResponse) =
    Json.write "ha_enabled" se.haEnabled
    *> Json.write "is_self" se.isSelf
    *> Json.write "leader_address" se.leaderAddress

type SecretWrapInfo = 
  { token         : Token
    ttl           : int
    creationTime  : string
  }

  
  static member FromJson (_: SecretWrapInfo) =
    (fun t d c ->
      { token = t
        ttl = d
        creationTime = c})
    <!> Json.read "token"
    <*> Json.read "ttl"
    <*> Json.read "creation_time"
    

  static member ToJson (se: SecretWrapInfo) =
    Json.write "token" se.token
    *> Json.write "ttl" se.ttl
    *> Json.write "creation_time" se.creationTime

type SecretAuth = 
  { clientToken   : Token
    accessor      : string
    policies      : string list
    metadata      : Map<string, string>
    leaseDuration : int
    renewable     : bool
  }

  static member FromJson (_: SecretAuth) =
    (fun ct a p m l r ->
      { clientToken = ct
        accessor = a
        policies = p
        metadata = m
        leaseDuration = l
        renewable = r})
    <!> Json.read "client_token"
    <*> Json.read "accessor"
    <*> Json.read "policies"
    <*> Json.read "metadata"
    <*> Json.read "lease_duration"
    <*> Json.read "renewable"
    

  static member ToJson (se: SecretAuth) =
    Json.write "client_token" se.clientToken
    *> Json.write "accessor" se.accessor
    *> Json.write "policies" se.policies
    *> Json.write "metadata" se.metadata
    *> Json.write "lease_duration" se.leaseDuration
    *> Json.write "renewable" se.renewable

type SecretDataList =
  { leaseId       : string
    leaseDuration : int
    renewable     : bool
    data          : Map<string, string list>
    warnings      : string list option
    auth          : SecretAuth option
    wrapInfo      : SecretWrapInfo option}

  static member FromJson (_: SecretDataList) =
    (fun li ld r d w a wi ->
      { leaseId = li
        leaseDuration = ld
        renewable = r
        data = d
        warnings = w
        auth = a
        wrapInfo = wi})
    <!> Json.read "lease_id"
    <*> Json.read "lease_duration"
    <*> Json.read "renewable"
    <*> Json.read "data"
    <*> Json.tryRead "warnings"
    <*> Json.tryRead "auth"
    <*> Json.tryRead "wrap_info"
    

  static member ToJson (se: SecretDataList) =
    Json.write "lease_id" se.leaseId
    *> Json.write "lease_duration" se.leaseDuration
    *> Json.write "renewable" se.renewable
    *> Json.write "data" se.data
    *> Json.maybeWrite "warnings" se.warnings
    *> Json.maybeWrite "auth" se.auth
    *> Json.maybeWrite "wrap_info" se.wrapInfo

type SecretDataString =
  { leaseId       : string
    leaseDuration : int
    renewable     : bool
    data          : Map<string, string>
    warnings      : string list option
    auth          : SecretAuth option
    wrapInfo      : SecretWrapInfo option}

  static member FromJson (_: SecretDataString) =
    (fun li ld r d w a wi ->
      { leaseId = li
        leaseDuration = ld
        renewable = r
        data = d
        warnings = w
        auth = a
        wrapInfo = wi})
    <!> Json.read "lease_id"
    <*> Json.read "lease_duration"
    <*> Json.read "renewable"
    <*> Json.read "data"
    <*> Json.tryRead "warnings"
    <*> Json.tryRead "auth"
    <*> Json.tryRead "wrap_info"
    

  static member ToJson (se: SecretDataString) =
    Json.write "lease_id" se.leaseId
    *> Json.write "lease_duration" se.leaseDuration
    *> Json.write "renewable" se.renewable
    *> Json.write "data" se.data
    *> Json.maybeWrite "warnings" se.warnings
    *> Json.maybeWrite "auth" se.auth
    *> Json.maybeWrite "wrap_info" se.wrapInfo


type KeyStatus =
  { term        : int
    installTime : string }

  static member FromJson (_: KeyStatus) =
    (fun t it ->
      { term = t
        installTime = it})
    <!> Json.read "term"
    <*> Json.read "install_time"
    

  static member ToJson (se: KeyStatus) =
    Json.write "term" se.term
    *> Json.write "install_time" se.installTime


type RekeyInitRequest =
  { secretShares    : int
    secretTreshold  : int
    pgpKeys         : string list option
    backup          : bool option}

  static member FromJson (_: RekeyInitRequest) =
    (fun ss st pgp b ->
      { secretShares = ss
        secretTreshold = st
        pgpKeys = pgp
        backup = b})
    <!> Json.read "secret_shares"
    <*> Json.read "secret_threshold"
    <*> Json.tryRead "pgp_keys"
    <*> Json.tryRead "backup"
    

  static member ToJson (se: RekeyInitRequest) =
    Json.write "secret_shares" se.secretShares
    *> Json.write "secret_threshold" se.secretTreshold
    *> Json.maybeWrite "pgp_keys" se.pgpKeys
    *> Json.maybeWrite "backup" se.backup


type RekeyStatusResponse =
  { nonce           : string
    started         : bool
    t               : int 
    n               : int
    progress        : int
    required        : int
    pgpFIngerPrints : string list option
    backup          : bool }

  static member FromJson (_: RekeyStatusResponse) =
    (fun nc s t n p r pgp b ->
      { nonce = nc
        started = s
        t = t
        n = n
        progress = p
        required = r
        pgpFIngerPrints = pgp
        backup = b})
    <!> Json.read "nonce"
    <*> Json.read "started"
    <*> Json.read "t"
    <*> Json.read "n"
    <*> Json.read "progress"
    <*> Json.read "required"
    <*> Json.tryRead "pgp_fingerprints"
    <*> Json.read "backup"
    

  static member ToJson (se: RekeyStatusResponse) =
    Json.write "nonce" se.nonce
    *> Json.write "started" se.started
    *> Json.write "t" se.t
    *> Json.write "n" se.n
    *> Json.write "progress" se.progress
    *> Json.write "required" se.required
    *> Json.maybeWrite "pgp_fingerprints" se.pgpFIngerPrints
    *> Json.write "backup" se.backup

type RekeyUpdateResponse =
  { nonce           : string
    complete        : bool
    keys            : string list
    pgpFIngerPrints : string list option
    backup          : bool }

  static member FromJson (_: RekeyUpdateResponse) =
    (fun nc c k pgp b ->
      { nonce = nc
        complete = c
        keys = k
        pgpFIngerPrints = pgp
        backup = b})
    <!> Json.read "nonce"
    <*> Json.read "complete"
    <*> Json.read "keys"
    <*> Json.tryRead "pgp_fingerprints"
    <*> Json.read "backup"
    

  static member ToJson (se: RekeyUpdateResponse) =
    Json.write "nonce" se.nonce
    *> Json.write "complete" se.complete
    *> Json.write "keys" se.keys
    *> Json.maybeWrite "pgp_fingerprints" se.pgpFIngerPrints
    *> Json.write "backup" se.backup

type RekeyRetrieveResponse =
  { nonce : string
    keys  : string list }

  static member FromJson (_: RekeyRetrieveResponse) =
    (fun t it ->
      { nonce = t
        keys = it})
    <!> Json.read "nonce"
    <*> Json.read "keys"
    

  static member ToJson (se: RekeyRetrieveResponse) =
    Json.write "nonce" se.nonce
    *> Json.write "keys" se.keys


type Audit =
  { path        : string option
    ``type``        : string
    description : string
    options     : Map<string, string> option} 

  static member FromJson (_: Audit) =
    (fun p t d o ->
      { path = p
        ``type`` = t
        description = d
        options = o})
    <!> Json.tryRead "file_path"
    <*> Json.read "type"
    <*> Json.read "description"
    <*> Json.tryRead "options"
    

  static member ToJson (se: Audit) =
    Json.maybeWrite "path" se.path
    *> Json.write "type" se.``type``
    *> Json.write "description" se.description
    *> Json.maybeWrite "options" se.options

type HealthResponse =
  { initialized   : bool
    ``sealed``    : bool
    standby       : bool
    serverTimeUtc : int option}

  static member FromJson (_: HealthResponse) =
    (fun p t d o ->
      { initialized = p
        ``sealed`` = t
        standby = d
        serverTimeUtc = o})
    <!> Json.read "initialized"
    <*> Json.read "sealed"
    <*> Json.read "standby"
    <*> Json.tryRead "server_time_utc"
    

  static member ToJson (se: HealthResponse) =
    Json.write "initialized" se.initialized
    *> Json.write "sealed" se.``sealed``
    *> Json.write "standby" se.standby
    *> Json.maybeWrite "server_time_utc" se.serverTimeUtc

type AuthConfig =
  { defaultLeaseTTL        : string  
    maxLeaseTTL            : string}

  static member FromJson (_: AuthConfig) =
    (fun p o ->
      { defaultLeaseTTL = p
        maxLeaseTTL = o})
    <!> Json.read "default_lease_ttl"
    <*> Json.read "max_lease_ttl"

  static member ToJson (se: AuthConfig) =
    Json.write "default_lease_ttl" se.defaultLeaseTTL
    *> Json.write "max_lease_ttl" se.maxLeaseTTL

type AuthMount =
  { ``type``        : string  
    description : string
    authConfig      : AuthConfig option}

  static member FromJson (_: AuthMount) =
    (fun p o c->
      { ``type`` = p
        description = o
        authConfig = c})
    <!> Json.read "type"
    <*> Json.read "description"
    <*> Json.tryRead "config"

  static member ToJson (se: AuthMount) =
    Json.write "type" se.``type``
    *> Json.write "description" se.description
    *> Json.maybeWrite "config" se.authConfig

type AuthMethod =
  | AppID
  | GitHub
  | LDAP
  | TLS
  | Tokens
  | UserPass
  | AWS
  with
    override x.ToString () =
      match x with
      | AppID -> "app-id"
      | GitHub -> "github"
      | LDAP -> "ldap"
      | TLS -> "cert"
      | Tokens -> "token"
      | UserPass -> "userpass"
      | AWS -> "aws-ec2"

    [<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>]
    static member FromString str =
      match str with
      | "app-id" -> AppID
      | "github" -> GitHub
      | "ldap" ->  LDAP
      | "cert" -> TLS
      | "token" -> Tokens
      | "userpass" -> UserPass
      | "aws-ec2" -> AWS
      | _ -> Tokens


