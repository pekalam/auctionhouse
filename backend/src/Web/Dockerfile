FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src

COPY ["Core.Common", "Core.Common/"]
COPY ["Core.Command", "Core.Command/"]
COPY ["Core.Query", "Core.Query/"]
COPY ["Infrastructure", "Infrastructure/"]
COPY ["Web", "Web/"]

FROM build AS publish
RUN dotnet publish "Web/Web.csproj" -c Release -o /app
COPY "Web/entrypoint.sh" /app
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT [ "./entrypoint.sh" ]