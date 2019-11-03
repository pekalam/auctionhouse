#!/bin/sh

docker cp "`pwd`/mongos/scripts" db-mongos1:/scripts
docker exec db-mongos1 bash -c 'mongo < /scripts/init.js'