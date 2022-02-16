#!/bin/bash

set -e 

while (( "$#" )); do 
  echo "Waiting for $1"
  ./wait-for "$1" -t 60
  shift 
done

exec dotnet Auctionhouse.CommandStatus.dll