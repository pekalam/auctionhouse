#!/bin/bash

set -e
docker-compose -f ./infrastructure/docker-compose.yml -f ./infrastructure/docker-compose.override.yml -f ./infrastructure/mongodb/docker-compose.yml -f ./infrastructure/mongodb/docker-compose.prod.env.yml \
-f ./webAPI/docker-compose.yml -f ./webAPI/docker-compose.override.yml \
-f ./front/docker-compose.yml -f ./front/docker-compose.override.yml up