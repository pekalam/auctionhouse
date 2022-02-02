$ver = $($args[0])
Write-Output @"
services:
  sql_server:
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Qwerty1234
      - MSSQL_AGENT_ENABLED=True
    ports:
      - '1433:1433'
    image: marekbf3/auctionhouse-sqlserver-backup:$ver
    healthcheck:
      test: "sqlcmd -S 127.0.0.1 -U sa -P 'Qwerty1234' -d AuctionhouseDatabase -Q 'select distinct 1 from dbo.Events' && sqlcmd -S 127.0.0.1 -U sa -P 'Qwerty1234' -d AuctionhouseDatabase -Q 'select distinct 1 from dbo.AuthData'"
"@