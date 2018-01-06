#!/usr/bin/env bash
set -e

if [[ ! -f vault ]]; then
  version=0.9.1
  file=vault_${version}_darwin_amd64.zip
  curl --silent -L https://releases.hashicorp.com/vault/${version}/${file} -o vault.zip
  unzip vault.zip
fi

export GOMAXPROCS=2
exec ./vault "$@"
