#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebAPI/Auctionhouse.Query/Auctionhouse.Query.csproj", "WebAPI/Auctionhouse.Query/"]
COPY ["Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Core.DomainFramework/Common.DomainFramework.csproj", "Core.DomainFramework/"]
COPY ["Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Auctions.Domain/Auctions.Domain.csproj", "Auctions.Domain/"]
COPY ["Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Auctions.Domain.Events/"]
COPY ["Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/"]
COPY ["ReadModel.Core/ReadModel.Core.csproj", "ReadModel.Core/"]
COPY ["Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["Users.DomainEvents/Users.DomainEvents.csproj", "Users.DomainEvents/"]
COPY ["AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "AuctionBids.DomainEvents/"]
COPY ["Categories.Domain/Categories.Domain.csproj", "Categories.Domain/"]
COPY ["UserPayments.Domain/UserPayments.Domain.csproj", "UserPayments.Domain/"]
COPY ["UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "UserPayments.DomainEvents/"]
COPY ["Adapters/EventBus/Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adapters/EventBus/Adatper.RabbitMq.EventBus/"]
COPY ["WebAPI/Common.WebAPI/Common.WebAPI.csproj", "WebAPI/Common.WebAPI/"]
COPY ["Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/"]
RUN dotnet restore "WebAPI/Auctionhouse.Query/Auctionhouse.Query.csproj"
COPY . .
WORKDIR "/src/WebAPI/Auctionhouse.Query"
RUN dotnet build "Auctionhouse.Query.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctionhouse.Query.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["WebAPI/Auctionhouse.Query/entrypoint.sh", "WebAPI/wait-for", "./"] 
RUN chmod +x entrypoint.sh wait-for
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]