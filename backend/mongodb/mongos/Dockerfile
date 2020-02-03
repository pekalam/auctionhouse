FROM mongo
RUN apt-get update && apt-get -y install netcat && apt-get -y install cron
EXPOSE 32112
COPY ["scripts", "/scripts/"]
COPY ["container-scripts", "/container-scripts/"]
COPY ["wait-for", "/bin/wait-for"]
COPY ["mongos-entrypoint.sh", "/root"]
COPY update-cronjob /root
RUN chmod +x /root/mongos-entrypoint.sh && chmod +x /bin/wait-for
COPY healthcheck.sh /bin/healthcheck.sh
RUN chmod +x /bin/healthcheck.sh
HEALTHCHECK --start-period=5m --interval=50s --retries=2 CMD healthcheck.sh || exit 1
ENTRYPOINT ["/root/mongos-entrypoint.sh"]