#!/bin/sh

docker cp "`pwd`/main/shard_1/scripts" db-node1:/scripts
docker exec db-node1 bash -c 'mongo appDb --port 27018 < /scripts/init.js'