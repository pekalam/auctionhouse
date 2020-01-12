FROM mongo
RUN apt-get update && apt-get -y install netcat
EXPOSE 32112
COPY ["scripts", "/scripts/"]
COPY ["wait-for", "/bin/wait-for"]
COPY ["mongo-entrypoint.sh", "/root"]
RUN chmod +x /root/mongo-entrypoint.sh && chmod +x /bin/wait-for
ENTRYPOINT ["/root/mongo-entrypoint.sh"]