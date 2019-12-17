FROM marekbf3/mssql-es
RUN apt-get update && apt-get -y install netcat
COPY entrypoint.sh /bin/entrypoint2.sh
COPY ./bin/Debug/AuthDataDatabase_Create.sql create.sql
RUN chmod +x /bin/entrypoint2.sh
EXPOSE 32112
ENTRYPOINT [ "entrypoint2.sh" ]
