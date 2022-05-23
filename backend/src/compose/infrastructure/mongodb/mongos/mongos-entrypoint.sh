#!/bin/bash

set -m
set -e

if [ -e "/run/secrets/mongocluster-keyfile" ]; then
    cp /run/secrets/mongocluster-keyfile /data/mongocluster-keyfile
    chmod 400 /data/mongocluster-keyfile
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
    echo "ARGS: $args"
fi

mongos $args & 

echo "waiting for port 27017"
wait-for localhost:27017 -t 180
echo "running init.js..."
mongosh 127.0.0.1:27017/admin /scripts/init.js

echo "mongos initialized"

echo "setting cronjob"
crontab /root/update-cronjob
echo "starting cron"
cron

echo "running update scripts..."
bash ./scripts/update-top-auctions-in-tag-view.sh
bash ./scripts/update-common-tags-view.sh
bash ./scripts/update-top-auctions-by-product-name.sh

echo "mongos started"
bash container-scripts/listen-on-health-port.sh &

fg %1
