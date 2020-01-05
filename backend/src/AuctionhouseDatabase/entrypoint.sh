#!/bin/bash

set -e

if [ -e "/run/secrets/sql_server_password" ]; then
	SA_PASSWORD=`< /run/secrets/sql_server_password`
fi

SA_PASSWORD=$SA_PASSWORD /opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

echo "Running docker_setup.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i docker_setup.sql

echo "Waiting for sqlagent..."
retry=10

while [ $retry -gt 0 ]; do
	result=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "exec msdb.dbo.sp_is_sqlagent_starting" 1>/dev/null 2>/dev/null && echo $? || echo $?)
	if [ $result -eq 0 ]; then
		break
	fi
	retry=$(($retry-1))
	sleep 5
done

if [ $retry -eq 0 ]; then
	echo "sql agent is not running"
	exit 128
fi


echo "Setting up db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i create.sql
echo "AuctionhouseDatabase created"

sleep 5

nc -l -s 0.0.0.0 -p 32112 &

wait