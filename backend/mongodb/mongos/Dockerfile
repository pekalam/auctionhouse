FROM mongo
RUN apt-get update && apt-get -y install netcat && apt-get -y install cron
EXPOSE 32112
COPY ["scripts", "/scripts/"]
COPY ["wait-for", "/bin/wait-for"]
COPY ["mongos-entrypoint.sh", "/root"]
COPY update-cronjob /root
RUN chmod +x /root/mongos-entrypoint.sh && chmod +x /bin/wait-for
ENTRYPOINT ["/root/mongos-entrypoint.sh"]