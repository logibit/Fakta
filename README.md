# Fakta

F# HTTP API for Consul.

## API

### Service


### KV


###  ...


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