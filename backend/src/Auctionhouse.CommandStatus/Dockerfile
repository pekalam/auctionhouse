#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y ncat

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Auctionhouse.CommandStatus/Auctionhouse.CommandStatus.csproj", "Auctionhouse.CommandStatus/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Core.DomainFramework/Common.DomainFramework.csproj", "Core.DomainFramework/"]
RUN dotnet restore "Auctionhouse.CommandStatus/Auctionhouse.CommandStatus.csproj"
COPY . .
WORKDIR "/src/Auctionhouse.CommandStatus"
RUN dotnet build "Auctionhouse.CommandStatus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctionhouse.CommandStatus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["Auctionhouse.CommandStatus/entrypoint.sh", "wait-for", "."] 
RUN chmod +x entrypoint.sh wait-for
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]