FROM mcr.microsoft.com/mssql/server:2017-CU8-ubuntu
RUN apt-get update && apt-get -y install netcat && apt-get -y install unzip
RUN wget -progress=bar:force -q -O sqlpackage.zip https://go.microsoft.com/fwlink/?linkid=2113331 \
    && unzip -qq sqlpackage.zip -d /opt/sqlpackage \
    && chmod +x /opt/sqlpackage/sqlpackage
COPY docker/wait-for /bin/wait-for
COPY docker/entrypoint-tests.sh /bin/entrypoint-tests.sh
COPY bin/Debug/AuctionhouseDatabase.dacpac .
COPY bin/Debug/Test.AuctionhouseDatabase.dacpac .
COPY bin/Debug/master.dacpac .
COPY docker_setup.sql .
COPY run_tests.sql .
RUN chmod +x /bin/entrypoint-tests.sh
RUN chmod +x /bin/wait-for

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=Qwerty1234
ENV MSSQL_AGENT_ENABLED=True

ENTRYPOINT [ "entrypoint-tests.sh" ]