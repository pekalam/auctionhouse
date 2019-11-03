#!/bin/sh

if [ "$1" = "dev" ] ; then
    configComposeFile="./config/docker-compose.dev.yml"
else
    configComposeFile="./config/docker-compose.yml"
fi


docker-compose -f ./mongos/docker-compose.yml -f $configComposeFile -f ./main/shard_1/docker-compose.yml -f ./main/shard_2/docker-compose.yml --project-directory . up -d