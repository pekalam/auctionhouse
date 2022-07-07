$cid = "$($args[0])"
$ver = "$($args[1])"

docker cp .\testbackups\"$ver"\dump.tar.gz "${cid}:/"
docker exec "${cid}" tar -xvzf dump.tar.gz
docker exec "${cid}" mongorestore /dump