#!/bin/bash

set -m
set -e

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

mongos $args & 

echo "waiting for port 27017"
wait-for localhost:27017 -t 180
echo "running init.js..."
mongo /scripts/init.js

echo "mongos initialized"

echo "setting cronjob"
crontab /root/update-cronjob
echo "starting cron"
cron

echo "running update scripts..."
./scripts/update-top-auctions-in-tag-view.sh
./scripts/update-common-tags-view.sh
./scripts/update-top-auctions-by-product-name.sh

echo "mongos started"
/container-scripts/listen-on-health-port.sh &

fg %1