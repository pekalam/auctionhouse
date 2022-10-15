$ErrorActionPreference = 'Stop'

xcopy '..\..\AuctionhouseDatabase\bin\Debug\*' ..\..\AuctionhouseDatabase.Docker\buildArtifacts /i /y

docker build --target build -t "${env:DOCKER_REGISTRY}pekalam/auctionhouse-sqlserver" ..\..\AuctionhouseDatabase.Docker

Remove-Item -Recurse ..\..\AuctionhouseDatabase.Docker\buildArtifacts