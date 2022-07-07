FROM mongo
RUN apt-get update && apt-get -y install netcat
COPY ["common/wait-for", "/bin/wait-for"]
COPY ["config/mongo-config-entrypoint.sh", "/root"]
RUN chmod +x /root/mongo-config-entrypoint.sh && chmod +x /bin/wait-for
COPY ["common", "/container-scripts/"]
COPY ["config/scripts", "/scripts/"]
EXPOSE 32112
ENTRYPOINT ["/root/mongo-config-entrypoint.sh"]