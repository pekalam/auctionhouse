#!/bin/bash

set -e
docker-compose --project-name 'auctionhouse' -f ./infrastructure/docker-compose.yml -f ./infrastructure/docker-compose.override.yml -f ./infrastructure/docker-compose.mongostandalone.yml up
