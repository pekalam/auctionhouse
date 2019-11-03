#!/bin/sh

docker cp "`pwd`/config/scripts" db-config1:/scripts
docker exec db-config1 bash -c 'mongo --port 27019 < /scripts/init.js'