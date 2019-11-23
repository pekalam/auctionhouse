#!/bin/bash

alias nc=ncat

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

crond &

crontab /root/update-cronjob

./root/scripts/update-top-auctions-in-tag-view.sh
./root/scripts/update-common-tags-view.sh
./root/scripts/update-top-auctions-by-product-name.sh

exec bash
