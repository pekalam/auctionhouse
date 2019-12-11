FROM marekbf3/mssql-es
COPY entrypoint.sh /bin/entrypoint2.sh
COPY ./bin/Debug/AuthDataDatabase_Create.sql create.sql
RUN chmod +x /bin/entrypoint2.sh
ENTRYPOINT [ "entrypoint2.sh" ]
