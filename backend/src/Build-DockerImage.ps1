xcopy 'AuctionhouseDatabase\bin\Debug\*' AuctionhouseDatabase.Docker\buildArtifacts /i /y

docker build --target build -t marekbf3/auctionhouse-sqlserver .\AuctionhouseDatabase.Docker

Remove-Item -Recurse .\AuctionhouseDatabase.Docker\buildArtifacts