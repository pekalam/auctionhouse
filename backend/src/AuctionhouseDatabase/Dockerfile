FROM mcr.microsoft.com/mssql/server:2017-CU8-ubuntu
RUN apt-get update && apt-get -y install netcat
COPY entrypoint.sh /bin/entrypoint2.sh
COPY ./bin/Debug/AuctionhouseDatabase_Create.sql create.sql
COPY docker_setup.sql .
COPY ["container-scripts", "/container-scripts/"]
RUN chmod +x /bin/entrypoint2.sh
COPY wait-for /bin/wait-for
RUN chmod +x /bin/wait-for
COPY healthcheck.sh /bin/healthcheck.sh
RUN chmod +x /bin/healthcheck.sh
HEALTHCHECK --start-period=10m --interval=50s --retries=2 CMD healthcheck.sh || exit 1
EXPOSE 32112
ENTRYPOINT [ "entrypoint2.sh" ]
