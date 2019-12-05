#!/bin/sh

SUCCESS=1
docker-compose -f ../docker-compose/docker-compose-functional-tests.yml up --build --exit-code-from auctionhouse-functional-tests auctionhouse-functional-tests || { SUCCESS=0; }
docker-compose -f ../docker-compose/docker-compose-functional-tests.yml down || { true; }

if [ "$SUCCESS" = 0 ]; then
	echo "Functional tests failed"
	exit 1
fi