FROM mongo
RUN apt-get update && apt-get -y install netcat
RUN apt-get -y install cron
COPY ["common/wait-for", "/bin/wait-for"]
COPY ["mongos/mongos-entrypoint.sh", "/root"]
RUN chmod +x /root/mongos-entrypoint.sh && chmod +x /bin/wait-for
COPY mongos/update-cronjob /root
COPY ["common", "/container-scripts/"]
COPY mongos/healthcheck.sh /bin/healthcheck.sh
RUN chmod +x /bin/healthcheck.sh
COPY ["mongos/scripts", "/scripts/"]
EXPOSE 32112
HEALTHCHECK --start-period=5m --interval=50s --retries=2 CMD healthcheck.sh || exit 1
ENTRYPOINT ["/root/mongos-entrypoint.sh"]