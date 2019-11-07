FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Core.Common", "Core.Common/"]
COPY ["Core.Command", "Core.Command/"]
COPY ["Core.Query", "Core.Query/"]
COPY ["Infrastructure", "Infrastructure/"]
COPY ["Tests", "Tests/"]
RUN dotnet build "Tests/FunctionalTests.csproj"
RUN apt-get update
RUN apt-get -y install netcat
COPY ["Tests/wait-for", "/bin/wait-for"]
RUN chmod +x /bin/wait-for
COPY ["Tests/wait-for-services", "/bin/wait-for-services"]
RUN chmod +x /bin/wait-for-services
EXPOSE 9998
WORKDIR "/src/Tests"
CMD ["dotnet", "test", "--no-build", "-s", "docker.runsettings"] 