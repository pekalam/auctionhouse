#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Auctionhouse.Query/Auctionhouse.Query.csproj", "Auctionhouse.Query/"]
COPY ["Adapter.EfCore.SagaEventsConfirmation/Adapter.EfCore.ReadModelNotifications.csproj", "Adapter.EfCore.SagaEventsConfirmation/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Core.DomainFramework/Common.DomainFramework.csproj", "Core.DomainFramework/"]
COPY ["Adapter.SqlServer.EventOutbox/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapter.SqlServer.EventOutbox/"]
COPY ["ReadModel.Core/ReadModel.Core.csproj", "ReadModel.Core/"]
COPY ["Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["Users.DomainEvents/Users.DomainEvents.csproj", "Users.DomainEvents/"]
COPY ["Auctions.Domain/Auctions.Domain.csproj", "Auctions.Domain/"]
COPY ["Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Auctions.Domain.Events/"]
COPY ["AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "AuctionBids.DomainEvents/"]
COPY ["Categories.Domain/Categories.Domain.csproj", "Categories.Domain/"]
COPY ["UserPayments.Domain/UserPayments.Domain.csproj", "UserPayments.Domain/"]
COPY ["UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "UserPayments.DomainEvents/"]
COPY ["Adapter.MongoDb/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapter.MongoDb/"]
COPY ["Xml.CategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Xml.CategoryTreeStore/"]
COPY ["RabbitMq/Adatper.RabbitMq.EventBus.csproj", "RabbitMq/"]
COPY ["Common.WebAPI/Common.WebAPI.csproj", "Common.WebAPI/"]
RUN dotnet restore "Auctionhouse.Query/Auctionhouse.Query.csproj"
COPY . .
WORKDIR "/src/Auctionhouse.Query"
RUN dotnet build "Auctionhouse.Query.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctionhouse.Query.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["Auctionhouse.Query/entrypoint.sh", "wait-for", "."] 
RUN chmod +x entrypoint.sh wait-for
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]