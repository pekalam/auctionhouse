#!/bin/bash

set -e

cd /src

for proj in $(find . -name '*.csproj'); do
  if [[ "$proj" == *"Database"* ]]; then
    echo "Ignoring $proj"
    continue
  fi

  echo "Executing dotnet restore command for $proj" 
  dotnet restore "$proj"
done
