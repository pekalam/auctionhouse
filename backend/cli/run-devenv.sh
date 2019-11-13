#!/bin/sh

echo "Starting mongodb dev..."
pushd ../mongodb
./run.sh dev
popd

echo "Starting rabbitmq, eventstore, quartzWebTaskService..."
pushd ../docker-compose
docker-compose -f docker-compose-devenv.yml up -d
popd