#!/bin/sh

docker cp "`pwd`/main/shard_2/scripts" db-node2:/scripts
docker exec db-node2 bash -c 'mongo --port 27018 < /scripts/init.js'