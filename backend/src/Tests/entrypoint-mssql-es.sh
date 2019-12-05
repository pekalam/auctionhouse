#!/bin/bash

set -e

/opt/mssql/bin/sqlservr &

sleep 30

echo "Setting up db..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i setup_db.sql
/opt/mssql-tools/bin/sqlcmd -d es -S localhost -U sa -P $SA_PASSWORD -i mssql-eventstore.sql
echo "es database created"

nc -l -s 0.0.0.0 -p 32112

wait