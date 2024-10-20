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
COPY ["Modules/Common/Common.Application/Common.Application.csproj", "Modules/Common/Common.Application/"]
COPY ["Modules/Common/Core.DomainFramework/Common.DomainFramework.csproj", "Modules/Common/Core.DomainFramework/"]
COPY ["Modules/ReadModel/ReadModel.Core/ReadModel.Core.csproj", "Modules/ReadModel/ReadModel.Core/"]
COPY ["Modules/Auctions/Auctions.Domain/Auctions.Domain.csproj", "Modules/Auctions/Auctions.Domain/"]
COPY ["Modules/Auctions/Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Modules/Auctions/Auctions.Domain.Events/"]
COPY ["Modules/Users/Users.Domain/Users.Domain.csproj", "Modules/Users/Users.Domain/"]
COPY ["Modules/Users/Users.DomainEvents/Users.DomainEvents.csproj", "Modules/Users/Users.DomainEvents/"]
COPY ["Modules/Categories/Categories.Domain/Categories.Domain.csproj", "Modules/Categories/Categories.Domain/"]
COPY ["Modules/UserPayments/UserPayments.Domain/UserPayments.Domain.csproj", "Modules/UserPayments/UserPayments.Domain/"]
COPY ["Modules/UserPayments/UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "Modules/UserPayments/UserPayments.DomainEvents/"]
COPY ["Modules/AuctionBids/AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "Modules/AuctionBids/AuctionBids.DomainEvents/"]
COPY ["Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/"]
COPY ["Adapters/EventBus/Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adapters/EventBus/Adatper.RabbitMq.EventBus/"]
COPY ["WebAPI/Common.WebAPI/WebAPI.Common.csproj", "WebAPI/Common.WebAPI/"]
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