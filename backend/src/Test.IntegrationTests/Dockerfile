FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Core.Common", "Core.Common/"]
COPY ["Core.Command", "Core.Command/"]
COPY ["Core.Query", "Core.Query/"]
COPY ["Infrastructure", "Infrastructure/"]
COPY ["Test.IntegrationTests", "Test.IntegrationTests/"]
RUN dotnet build "Test.IntegrationTests/Test.IntegrationTests.csproj"
RUN apt-get update
RUN apt-get -y install netcat
COPY ["Test.IntegrationTests/wait-for", "/bin/wait-for"]
RUN chmod +x /bin/wait-for
COPY ["Test.IntegrationTests/wait-for-services", "/bin/wait-for-services"]
RUN chmod +x /bin/wait-for-services
EXPOSE 9998
WORKDIR "/src/Test.IntegrationTests"
CMD ["dotnet", "test", "--no-build", "-s", "docker.runsettings"] 