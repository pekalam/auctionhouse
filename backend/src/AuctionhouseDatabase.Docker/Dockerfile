FROM mcr.microsoft.com/mssql/server:2017-CU8-ubuntu as build
RUN apt-get update && apt-get -y install netcat
COPY ["container-scripts", "/container-scripts/"]
COPY entrypoint.sh /bin/custom-entrypoint.sh
RUN chmod +x /bin/custom-entrypoint.sh
COPY wait-for /bin/wait-for
RUN chmod +x /bin/wait-for

COPY healthcheck.sh /bin/healthcheck.sh
RUN chmod +x /bin/healthcheck.sh
HEALTHCHECK --interval=30s --retries=2 CMD healthcheck.sh || exit 1

COPY docker_setup.sql .
COPY buildArtifacts/AuctionhouseDatabase_Create.sql create.sql

EXPOSE 32112
ENTRYPOINT [ "custom-entrypoint.sh" ]


FROM build as frombackup
COPY restore-AuctionhouseDatabase.sql ./
COPY frombackup-entrypoint.sh /bin/frombackup-entrypoint.sh
RUN chmod +x /bin/frombackup-entrypoint.sh
ARG BACKUP_LOCATION
COPY ${BACKUP_LOCATION}AuctionhouseDatabase.mdf ${BACKUP_LOCATION}AuctionhouseDatabase_log.ldf ./
ENTRYPOINT [ "frombackup-entrypoint.sh" ]