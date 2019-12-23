#!/bin/sh

echo "Starting mongodb dev..."
pushd ../mongodb
./run.sh dev $1 $2
popd
shift 2

echo "Starting rabbitmq, eventstore, quartzWebTaskService..."
pushd ../docker-compose
docker-compose -f docker-compose-devenv.yml up -d $@
popd