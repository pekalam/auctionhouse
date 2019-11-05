#!/bin/sh

if [ -z "$1" ]
then
	NETWORK="mongodb_default"
else
	NETWORK="$1"
fi

docker-compose -f ./views/docker-compose.yml --project-directory . up -d




