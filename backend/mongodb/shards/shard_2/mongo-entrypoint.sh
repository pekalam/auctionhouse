#!/bin/bash

set -m
set -e

if [ -e "/run/secrets/mongo_password" ]; then
	MONGO_INITDB_ROOT_PASSWORD=`< /run/secrets/mongo_password`
fi

if [ "$1" == "--wait-for" ]; then
    shift 1
    args=($@)
    for ((i=0;i<${#args[@]};i++)); do
        arg="${args[i]}"
        if [ "${arg:0:2}" == "--" ] || [ "${arg:0:1}" == "-" ]; then
            shift $i
            args=$@
            break
        fi
        echo "waiting for $arg..."
        wait-for "$arg" -t 180
    done
else
    args=$@
fi

MONGO_INITDB_ROOT_PASSWORD=$MONGO_INITDB_ROOT_PASSWORD mongod $args & 

echo "waiting for port 27018"
wait-for localhost:27018 -t 180
echo "running init.js..."
mongo appDb --port 27018 /scripts/init.js

echo "mongodb initialized"
nc -l -k -s 0.0.0.0 -p 32112 &
fg %1