#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

echo "Copying database files to sql server data folder"
cp /AuctionhouseDatabase.mdf /var/opt/mssql/data
cp /AuctionhouseDatabase_log.ldf /var/opt/mssql/data

echo "Creating auctionhouseDatabase db..."

success=1
max=3
set +e
while [ "$success" -ne 0 ]; do 
	/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -i restore-AuctionhouseDatabase.sql
	success=$?
	if [ "$success" -eq 0 ]; then
		break
	fi
	max=$((max-1))
	if [ "$max" -eq 0 ]; then
		echo "Reached max retry count"
		exit 128
	fi
	echo "Could restore db. Retrying in 10 sec..."
	sleep 10
done

set -e
echo "AuctionhouseDatabase db created"

wait