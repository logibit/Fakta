#!/usr/bin/env bash
set -e

# see https://www.consul.io/downloads.html

if [[ ! -f consul ]]; then
  version=0.6.4
  file=consul_${version}_darwin_amd64.zip
  curl --silent -L https://releases.hashicorp.com/consul/${version}/${file} -o consul.zip
  unzip consul.zip
fi

export GOMAXPROCS=2
exec ./consul "$@"
