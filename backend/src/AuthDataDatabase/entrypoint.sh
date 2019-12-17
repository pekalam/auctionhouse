#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

wait-for 0.0.0.0:1433 -t 240

echo "Setting up db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i setup_db.sql
/opt/mssql-tools/bin/sqlcmd -d es -S localhost -U sa -P $SA_PASSWORD -i mssql-eventstore.sql
echo "es database created"

echo "Creating authData db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /create.sql
echo "AuthData db created"

sleep 5

nc -l -s 0.0.0.0 -p 32112 &

wait