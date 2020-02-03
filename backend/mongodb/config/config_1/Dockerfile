FROM mongo
RUN apt-get update && apt-get -y install netcat
EXPOSE 32112
COPY ["scripts", "/scripts/"]
COPY ["container-scripts", "/container-scripts/"]
COPY ["wait-for", "/bin/wait-for"]
COPY ["mongo-config-entrypoint.sh", "/root"]
RUN ls /root
RUN chmod +x /root/mongo-config-entrypoint.sh && chmod +x /bin/wait-for
ENTRYPOINT ["/root/mongo-config-entrypoint.sh"]