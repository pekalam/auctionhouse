#!/bin/sh

if [ "$1" = "dev" ] ; then
    configComposeFile="./config/docker-compose.dev.yml"
	shift 1
else
    configComposeFile="./config/docker-compose.yml"
fi

docker-compose \
-f ./mongos/docker-compose.yml \
-f $configComposeFile \
-f ./shards/shard_1/docker-compose.yml \
-f ./shards/shard_2/docker-compose.yml \
-f ./views/docker-compose.yml \
--project-directory . \
up -d