#!/bin/sh

SUCCESS=1
docker-compose -f ../docker-compose/docker-compose-integration-tests.yml up --build --exit-code-from auctionhouse-integration-tests || { SUCCESS=0; }
docker-compose -f ../docker-compose/docker-compose-integration-tests.yml down || { true; }

if [ "$SUCCESS" = 0 ]; then
	echo "Integration tests failed"
	exit 1
fi