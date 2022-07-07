#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

echo "Running docker_setup.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i docker_setup.sql

echo "creating database..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "CREATE DATABASE AuctionhouseDatabase"

retry=10

while [ $retry -gt 0 ]; do
	result=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "exec msdb.dbo.sp_is_sqlagent_starting" 1>/dev/null 2>/dev/null && echo $? || echo $?)
	if [ $result -eq 0 ]; then
		break
	fi
	retry=$(($retry-1))
	sleep 5
done

echo "Publishing Test.AuctionhouseDatabase.dacpac package"
/opt/sqlpackage/sqlpackage /Action:Publish /SourceFile:Test.AuctionhouseDatabase.dacpac /TargetDatabaseName:AuctionhouseDatabase /TargetServerName:"localhost" /TargetUser:"sa" /TargetPassword:"Qwerty1234"

echo "Publishing AuctionhouseDatabase.dacpac package"
/opt/sqlpackage/sqlpackage /Action:Publish /SourceFile:AuctionhouseDatabase.dacpac /TargetDatabaseName:AuctionhouseDatabase /TargetServerName:"localhost" /TargetUser:"sa" /TargetPassword:"Qwerty1234"

sleep 5

echo "running tests..."
/opt/mssql-tools/bin/sqlcmd -d AuctionhouseDatabase -S localhost -U sa -P $SA_PASSWORD -i run_tests.sql

if [ $? -eq 0 ]; then
    exit 128
fi

exit