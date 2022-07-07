#!/bin/bash

set -e

if [ -e "/run/secrets/sql_server_password" ]; then
	SA_PASSWORD=`< /run/secrets/sql_server_password`
fi

if nc -zv 127.0.0.1 32112; then
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U sa -P "$SA_PASSWORD" -d AuctionhouseDatabase -Q "select distinct 1 from dbo.Event"
fi