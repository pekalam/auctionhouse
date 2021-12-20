FROM mcr.microsoft.com/dotnet/runtime:3.1.22-alpine3.14 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1.416-alpine3.14 AS build
WORKDIR /src
COPY ["Core.DomainFramework", "Core.DomainFramework/"]
COPY ["Core.Common", "Core.Common/"]
COPY ["Core.Command", "Core.Command/"]
COPY ["Core.Query", "Core.Query/"]
COPY ["Infrastructure", "Infrastructure/"]
COPY ["Test.IntegrationTests", "Test.IntegrationTests/"]
RUN dotnet build "Test.IntegrationTests/Test.IntegrationTests.csproj"
RUN apk update
RUN apk add netcat-openbsd
RUN apk add bash
COPY ["Test.IntegrationTests/wait-for", "/bin/wait-for"]
RUN chmod +x /bin/wait-for
COPY ["Test.IntegrationTests/wait-for-services", "/bin/wait-for-services"]
RUN chmod +x /bin/wait-for-services
EXPOSE 9998
WORKDIR "/src/Test.IntegrationTests"
CMD ["dotnet", "test", "--no-build", "-s", "docker.runsettings"] 