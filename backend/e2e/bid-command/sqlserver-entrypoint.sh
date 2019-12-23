#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

echo "Creating auctionhouseDatabase db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i restore-AuctionhouseDatabase.sql
echo "AuctionhouseDatabase db created"

wait