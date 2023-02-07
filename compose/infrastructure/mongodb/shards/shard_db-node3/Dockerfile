FROM mongo:5.0.6
RUN apt-get update && apt-get -y install netcat
COPY ["common/wait-for", "/bin/wait-for"]
COPY ["shards/shard_db-node3/mongo-entrypoint.sh", "/root"]
RUN chmod +x /root/mongo-entrypoint.sh && chmod +x /bin/wait-for
COPY ["common", "/container-scripts/"]
EXPOSE 32112
COPY ["shards/shard_db-node3/scripts", "/scripts/"]
ENTRYPOINT ["/root/mongo-entrypoint.sh"]