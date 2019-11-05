#!/bin/sh

pushd ../mongodb/views/docker
docker build -f Dockerfile -t update-server .
popd