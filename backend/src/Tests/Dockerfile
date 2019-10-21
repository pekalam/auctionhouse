FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj", "Infrastructure.IntegrationTests/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "Domain/Domain.csproj"
RUN dotnet restore "Infrastructure/Infrastructure.csproj"
RUN dotnet restore "Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/Domain"
RUN dotnet build
WORKDIR "/src/Infrastructure"
RUN dotnet build
WORKDIR "/src/Infrastructure.IntegrationTests"
RUN dotnet build "Infrastructure.IntegrationTests.csproj"

WORKDIR "/src/Infrastructure.IntegrationTests"
ENTRYPOINT ["dotnet", "test", "-v", "d", ".", "--", "-noshadow"]