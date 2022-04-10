docker-compose --project-name 'auctionhouse' -f .\infrastructure\docker-compose.yml -f .\infrastructure\docker-compose.override.yml `
-f .\webAPI\docker-compose.yml -f .\webAPI\docker-compose.override.yml `
-f .\front\docker-compose.yml -f .\front\docker-compose.override.yml up