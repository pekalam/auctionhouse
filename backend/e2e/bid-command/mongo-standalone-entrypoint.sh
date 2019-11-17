#!/bin/bash
set -m

mongod --replSet rs0 --bind_ip_all &

maxTry=10
i=0
while [ -z "`netcat -v -z 0.0.0.0 27017 2>&1 | grep open`" ] ; do
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
while [ -z "`netcat -v -z 0.0.0.0 27017 2>&1 | grep open`" ] ; do
	echo "Waiting for 27017 port to open";
	sleep 3;
	i=$((i+1))
	if [ $i -eq $maxTry ]; then
		echo "Cannot connect to 27017 port"
		exit 1;
	fi
done

for f in /root/appDb/*.gz; do mongorestore $f --gzip; done

echo "mongodb initialized"

nc -l -s 0.0.0.0 -p 32112 &

fg %1