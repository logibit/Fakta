#!/usr/bin/env bash
set -e

# see https://www.consul.io/downloads.html

if [[ ! -f consul ]]; then
  file=0.5.2_darwin_amd64.zip
  curl --silent -L  https://dl.bintray.com/mitchellh/consul/$file -o consul.zip
  unzip consul.zip
fi

export GOMAXPROCS=2
exec ./consul "$@"
