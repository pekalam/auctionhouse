if [ "$1" = "dev" ] ; then
    configComposeFile="./config/docker-compose.dev.yml"
else
    configComposeFile="./config/docker-compose.yml"
fi


docker-compose -f ./mongos/docker-compose.yml -f $configComposeFile -f ./main/shard_1/docker-compose.yml -f ./main/shard_2/docker-compose.yml --project-directory . up -d
sleep 10
echo "shard_1 init"
./main/shard_1/init.sh
echo "shard_2 init"
./main/shard_2/init.sh
echo "configdb init"
./config/init.sh
echo "mongos ini"
sleep 10
./mongos/init.sh
