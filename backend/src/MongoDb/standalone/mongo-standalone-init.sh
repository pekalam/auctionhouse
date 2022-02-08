#!/bin/bash
set -m


RS_STATUS=$(mongo --eval "rs.status().ok")
if [[ $RS_STATUS != 1 ]]; then
	mongo --eval "rs.initiate()"
else
	echo "Replicaset already initialized"
	mongo appDb --eval 'config = rs.config();
config.members[0].host = "mongodb;"
rs.reconfig(config, {force: true});'
fi

mongo appDb --eval '
db.createCollection("AuctionsReadModel");
db.createCollection("UsersReadModel");'

echo "mongodb initialized"
