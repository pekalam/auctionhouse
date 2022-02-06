xcopy 'AuctionhouseDatabase\bin\Debug\*' AuctionhouseDatabase.Docker\buildArtifacts /i /y

docker build --target build -t pekalam/auctionhouse-sqlserver .\AuctionhouseDatabase.Docker

Remove-Item -Recurse .\AuctionhouseDatabase.Docker\buildArtifacts