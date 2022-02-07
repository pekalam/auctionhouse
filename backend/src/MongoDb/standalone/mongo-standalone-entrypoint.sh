#!/bin/bash
set -m

mongod --replSet rs0 --bind_ip_all &

maxTry=10
i=0
while ! netcat -zv 0.0.0.0 27017; do
	echo "Waiting for 27017 port to open";
	sleep 3;
	i=$((i+1))
	if [ $i -eq $maxTry ]; then
		echo "Cannot connect to 27017 port"
		exit 1;
	fi
done

mongo appDb /root/init.js

maxTry=10
i=0
while ! netcat -zv 0.0.0.0 27017; do
	echo "Waiting for 27017 port to open";
	sleep 3;
	i=$((i+1))
	if [ $i -eq $maxTry ]; then
		echo "Cannot connect to 27017 port"
		exit 1;
	fi
done


mongo appDb /root/init-collections.js

echo "mongodb initialized"

echo "setting cronjob"
crontab /root/update-cronjob
echo "starting cron"
cron

echo "running update scripts..."
./scripts/update-top-auctions-in-tag-view.sh
./scripts/update-common-tags-view.sh
./scripts/update-top-auctions-by-product-name.sh


nc -l -s 0.0.0.0 -p 32112 &

fg %1