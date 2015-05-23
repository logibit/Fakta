# Fakta

*The* F# HTTP API for Consul.

Fakta is Swedish for 'facts', a fitting name for a library interacting with a
CP-oriented fact store.

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

### [KV][docs-KV]

 - [ ] acquire
 - [ ] CAS
 - [ ] delete
 - [ ] deleteCAS
 - [ ] deleteTree
 - [ ] getRaw
 - [x] get
 - [ ] keys
 - [x] list
 - [x] put
 - [ ] release

### [Session][docs-Session]

 - [x] create
 - [ ] createNoChecks
 - [x] destroy
 - [ ] info
 - [ ] list
 - [ ] node
 - [ ] renew
 - [ ] renewPeriodic

### Service


## Helping Out

All development is done on `master` branch which should always be release-able;
write unit tests for your changes and it shall be fine.

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
 [reg]: https://github.com/gliderlabs/registrator