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

echo "waiting for port 27019"
wait-for localhost:27019 -t 180
echo "running init.js..."
mongo --port 27019 /scripts/init.js

echo "mongo configsvr initialized"
bash container-scripts/listen-on-health-port.sh &
fg %1