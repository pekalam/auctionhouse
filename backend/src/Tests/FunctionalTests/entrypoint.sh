#!/bin/bash

set -e

APP_ENV="$1"
shift

while (( "$#" )); do 
  echo "Waiting for $1"
  wait-for "$1" -t 60
  shift 
done

sleep 30
exec dotnet test FunctionalTests.csproj --no-restore -e APP_ENV="$APP_ENV"