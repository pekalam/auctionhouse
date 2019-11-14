#!/bin/sh

if [ "$1" = "dev" ]; then
    dev=".dev"
    shift 1
else
    dev=""
fi

if [ "$1" = "swarm" ]; then
    if [ "$2" != "no-build" ]; then
        docker-compose \
            -f ./mongos/docker-compose$dev.yml \
            -f ./config/config_1/docker-compose$dev.yml \
            -f ./shards/shard_1/docker-compose$dev.yml \
            -f ./shards/shard_2/docker-compose$dev.yml \
            -f ./views/docker-compose$dev.yml \
            --project-directory . \
            build
    fi
    if [ "$2" != "no-deploy" ]; then
        docker stack deploy --compose-file ./docker-compose-swarm.dev.yml mongodb_stack
    fi
else
    docker-compose \
        -f ./mongos/docker-compose$dev.yml \
        -f ./config/config_1/docker-compose$dev.yml \
        -f ./shards/shard_1/docker-compose$dev.yml \
        -f ./shards/shard_2/docker-compose$dev.yml \
        -f ./views/docker-compose$dev.yml \
        --project-directory . \
        up -d "$@"
fi
