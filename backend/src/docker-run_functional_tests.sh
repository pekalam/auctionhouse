#!/bin/sh

set -e

if [ -e ./quartzWebTaskService ]; then
	rm -rf ./quartzWebTaskService
fi

git clone https://github.com/pekalam/quartzWebTaskService.git

set +e

SUCCESS=1
docker-compose -f docker-compose-functional-tests.yml up auctionhouse-functional-tests || { SUCCESS=0; }
docker-compose -f docker-compose-functional-tests.yml down || { true; }

if [ "$SUCCESS" = 0 ]; then
	echo "Functional tests failed"
	exit 1
fi