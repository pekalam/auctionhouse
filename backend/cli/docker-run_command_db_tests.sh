#!/bin/sh

SUCCESS=1
docker-compose -f ../docker-compose/docker-compose-command-db-tests.yml up --build --exit-code-from auctionhouse-database-tests || { SUCCESS=0; }
docker-compose -f ../docker-compose/docker-compose-command-db-tests.yml down || { true; }

if [ "$SUCCESS" = 0 ]; then
	echo "Command db tests failed"
	exit 1
fi