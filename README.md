# Fakta

A Consul and Vault F# API.

The aim is to support both Consul and Vault from the same library, because it's
a common deployment scenario. You should be able to use the Consul bits without
using the Vault bits.

Fakta is Swedish for 'facts', a fitting name for a library interacting with a
CP-oriented fact store.

Sponsored by
[qvitoo – A.I. bookkeeping](https://qvitoo.com/?utm_source=github&utm_campaign=fakta).

## API

How much has been implemented? This API surface is the same as that of the
official [Go client][go-client].

Those not implemented will throw a correspond TBD-exception.

### Justification

The current implementation's use-case (for me) is [leader-election][docs-LE] and semi- to
long-term storage of access keys that need be requested exactly-once or they
get invalidated.

Together with [Registrator][reg] and this library, F# code can participate in micro-
service architectures easily.

## Compiling and running initial tests

First, run:

``` bash
./tools/consul.sh agent -dev -bind 127.0.0.1 -config-file=server.json
```

Then in another terminal:

``` bash
bundle exec rake
```

Which will call xbuild/msbuild and compile the project, run unit tests and then
finally run the integration tests.

## Milestones

[All Milestones](https://github.com/haf/Fakta/milestones)

Prio 1 is what is needed to get a PoC up and running.
Prio 2 is next, by being good to have.
Prio 3 is next.

The order of Consul vs Vault priorities is:

 - Vault Prio 1
 - Consul Prio 1
 - Vault Prio 2
 - Consul Prio 2
 - etc

Note that Vault is on top, because all the Prio 0's of Consul are already done.

## References

### Consul

 - [The HTTP API](https://www.consul.io/docs/agent/http.html)
 - [go-client](https://godoc.org/github.com/hashicorp/consul/api) – the library
   is partially modelled after this

### Vault

 - [Official Ruby Client](https://github.com/hashicorp/vault-ruby/tree/master/lib/vault/api)
 - [Official Ruby Client Docs](http://www.rubydoc.info/gems/vault/0.1.5)
 - [The HTTP API](https://vaultproject.io/docs/http/index.html)

### [ACL][docs-Acl]

 - [x] clone
 - [x] create
 - [x] destroy
 - [x] info
 - [x] list
 - [x] update

### [Agent][docs-Agent]

 - [x] checkRegister
 - [x] checkDeregister
 - [x] checks
 - [x] enableNodeMaintenance
 - [x] disableNodeMaintenance
 - [x] enableServiceMaintenance
 - [x] disableServiceMaintenance
 - [x] join
 - [x] members
 - [x] self
 - [x] serviceRegister
 - [x] serviceDeregister
 - [x] nodeName
 - [x] services
 - [x] passTTL
 - [x] warnTTL
 - [x] failTTL
 - [x] forceLeave
 
### [Catalog][docs-Catalog]

 - [x] datacenters
 - [x] node
 - [x] nodes
 - [x] deregister
 - [x] register
 - [x] service
 - [x] services

### [Event][docs-Event]

 - [x] fire
 - [x] list
 - [x] idToIndex
 
### [Health][docs-Health]

 - [x] checks
 - [x] node
 - [x] state
 - [x] service

### [KV][docs-KV]

 - [x] acquire
 - [x] CAS
 - [x] delete
 - [x] deleteCAS
 - [x] deleteTree
 - [x] getRaw
 - [x] get
 - [x] keys
 - [x] list
 - [x] put
 - [x] release

### [Session][docs-Session]

 - [x] create
 - [x] createNoChecks
 - [x] destroy
 - [x] info
 - [x] list
 - [x] node
 - [x] renew
 - [x] renewPeriodic

### [Status][docs-Status]
 - [x] leader
 - [x] peers

Service

## Helping Out

All development is done on `master` branch which should always be release-able;
write unit tests for your changes and it shall be fine.

### Running local instance of Consul 
1. enable [ACL support][acl-support] by creating json config file "server.json" looking like this:
 ```
 {
  "bootstrap": false,
  "server": true,
  "datacenter": "ams1",
  "encrypt": "yJqVBxe12ZfE3z+4QSk8qA==",
  "log_level": "INFO",
  "acl_datacenter": "ams1",
  "acl_default_policy": "allow",
  "acl_master_token": "secret",
  "acl_token": "secret"
 }
 ```
2. run consul agent with: 
 ```
 consul agent -dev -bind 127.0.0.1 -config-file=path to server.json
 ```
3. Open *http://localhost:consul_port/ui/#/ams1/acls (typically http://127.0.0.1:8500/ui/#/ams1/acls )* and create token called the same like the master token in config file

### Running local instance of Consul 
1. create config file (do not use this in production)
 ```
 backend "file" {
   path = "vault"
 }
 
 listener "tcp" {
   tls_disable = 1
 }
 
 disable_cache = true
 disable_mlock = true
 ```
2. run vault server with
  ```vault server -config=config_file_name.conf```
3. you have to restart your local vault server and delete vault directory contents everytime you want to re-run tests.

### Compiling

You compile with Rake/albacore, the best build system for .Net/mono:

```
bundle
bundle exec rake
```

### Running Tests

The unit tests are run by just running the Tests-executable, or by using rake:

```
bundle exec rake tests:unit
```

### Running Integration Tests

You'll need to start consul first; in one console:

```
bundle exec foreman start
```

...and then running the tests:

```
bundle exec rake tests:integration
```

 [go-client]: https://godoc.org/github.com/hashicorp/consul/api
 [docs-LE]: https://www.consul.io/docs/guides/leader-election.html
 [docs-KV]: https://www.consul.io/docs/agent/http/kv.html
 [docs-Session]: https://www.consul.io/docs/agent/http/session.html
 [docs-Status]: https://www.consul.io/docs/agent/http/status.html
 [docs-Acl]: https://www.consul.io/docs/agent/http/acl.html
 [docs-Agent]: https://www.consul.io/docs/agent/http/agent.html
 [docs-Catalog]: https://www.consul.io/docs/agent/http/catalog.html
 [docs-Event]: https://www.consul.io/docs/agent/http/event.html
 [docs-Health]: https://www.consul.io/docs/agent/http/health.html
 [acl-support]: https://www.consul.io/docs/agent/options.html
 [reg]: https://github.com/gliderlabs/registrator
