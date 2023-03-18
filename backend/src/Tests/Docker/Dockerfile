# build context path: backend\src

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat


COPY ["Tests/Docker/wait-for", "/bin/wait-for"]
RUN chmod +x /bin/wait-for

COPY ["Tests/Docker/restore-proj.sh", "/bin/restore-proj.sh"]
RUN chmod +x /bin/restore-proj.sh

WORKDIR /src
COPY . .
RUN rm -rf /src/Database
RUN restore-proj.sh

COPY ["Tests/Docker/entrypoint.sh", "/bin/entrypoint.sh"]
RUN chmod +x /bin/entrypoint.sh


ENTRYPOINT [ "entrypoint.sh" ]