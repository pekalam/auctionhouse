FROM mcr.microsoft.com/mssql/server:2017-CU8-ubuntu
RUN apt-get update && apt-get -y install netcat
COPY entrypoint.sh /bin/entrypoint2.sh
COPY ./bin/Debug/AuctionhouseDatabase_Create.sql create.sql
COPY docker_setup.sql .
RUN chmod +x /bin/entrypoint2.sh
COPY wait-for /bin/wait-for
RUN chmod +x /bin/wait-for
EXPOSE 32112
ENTRYPOINT [ "entrypoint2.sh" ]
