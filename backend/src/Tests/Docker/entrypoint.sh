#!/bin/bash

set -e

APP_ENV="$1"
shift
echo "APP_ENV=$APP_ENV"

CONFIGURATION="$1"
shift
echo "CONFIGURATION=$CONFIGURATION"


while (( "$#" )); do 
  echo "Waiting for $1"
  wait-for "$1" -t 60
  shift 
done


cd /src

for proj in $(find . -name '*.csproj'); do
  if [[ "$proj" == *"Database"* ]]; then
    echo "Ignoring $proj"
    continue
  fi

  echo "Executing dotnet test command for $proj" 
  dotnet test "$proj" -c "$CONFIGURATION" -e APP_ENV="$APP_ENV" --filter "Category = Functional | Category = Integration" --no-restore
done
