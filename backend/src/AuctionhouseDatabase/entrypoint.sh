#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

retry=10

while [ $retry -gt 0 ]; do
	result=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "exec msdb.dbo.sp_is_sqlagent_starting" 1>/dev/null 2>/dev/null && echo $? || echo $?)
	if [ $result -eq 0 ]; then
		break
	fi
	retry=$(($retry-1))
	sleep 5
done


echo "Setting up db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i create.sql
echo "AuctionhouseDatabase created"

sleep 5

nc -l -s 0.0.0.0 -p 32112 &

wait