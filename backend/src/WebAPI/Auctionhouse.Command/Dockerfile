#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebAPI/Auctionhouse.Command/Auctionhouse.Command.csproj", "WebAPI/Auctionhouse.Command/"]
COPY ["Adapters/Adapter.ChronicleEfCoreStorage/ChronicleEfCoreStorage.csproj", "Adapters/Adapter.ChronicleEfCoreStorage/"]
COPY ["Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Core.DomainFramework/Common.DomainFramework.csproj", "Core.DomainFramework/"]
COPY ["IntegrationService.AuctionPaymentVerification/IntegrationService.AuctionPaymentVerification.csproj", "IntegrationService.AuctionPaymentVerification/"]
COPY ["Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["Users.DomainEvents/Users.DomainEvents.csproj", "Users.DomainEvents/"]
COPY ["Auctions.Domain/Auctions.Domain.csproj", "Auctions.Domain/"]
COPY ["Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Auctions.Domain.Events/"]
COPY ["UserPayments.Domain/UserPayments.Domain.csproj", "UserPayments.Domain/"]
COPY ["UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "UserPayments.DomainEvents/"]
COPY ["AuctionBids.Application/AuctionBids.Application.csproj", "AuctionBids.Application/"]
COPY ["AuctionBids.Domain/AuctionBids.Domain.csproj", "AuctionBids.Domain/"]
COPY ["AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "AuctionBids.DomainEvents/"]
COPY ["Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Adapters/Schedulers/Adapter.QuartzTimeTaskService.AuctionEndScheduler/Adapter.QuartzTimeTaskService.AuctionEndScheduler.csproj", "Adapters/Schedulers/Adapter.QuartzTimeTaskService.AuctionEndScheduler/"]
COPY ["Auctions.Application/Auctions.Application.csproj", "Auctions.Application/"]
COPY ["Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/"]
COPY ["Adapters/AuctionImageConversion/Adapter.AuctionImageConversion/Adapter.AuctionImageConversion.csproj", "Adapters/AuctionImageConversion/Adapter.AuctionImageConversion/"]
COPY ["UserPayments.Application/UserPayments.Application.csproj", "UserPayments.Application/"]
COPY ["Adapters/EventBus/Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adapters/EventBus/Adatper.RabbitMq.EventBus/"]
COPY ["Categories.Domain/Categories.Domain.csproj", "Categories.Domain/"]
COPY ["Users.Application/Users.Application.csproj", "Users.Application/"]
COPY ["WebAPI/Common.WebAPI/Common.WebAPI.csproj", "WebAPI/Common.WebAPI/"]
COPY ["Adapters/Repositories/Adapter.Dapper.AuctionhouseDatabase/Adapter.Dapper.AuctionhouseDatabase.csproj", "Adapters/Repositories/Adapter.Dapper.AuctionhouseDatabase/"]
COPY ["Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/"]
COPY ["Adapters/Schedulers/Adapter.Hangfire.Auctionhouse/Adapter.Hangfire_.Auctionhouse.csproj", "Adapters/Schedulers/Adapter.Hangfire.Auctionhouse/"]
COPY ["IntegrationService.CategoryNamesToTreeIdsConversion/IntegrationService.CategoryNamesToTreeIdsConversion.csproj", "IntegrationService.CategoryNamesToTreeIdsConversion/"]
RUN dotnet restore "WebAPI/Auctionhouse.Command/Auctionhouse.Command.csproj"
COPY . .
WORKDIR "/src/WebAPI/Auctionhouse.Command"
RUN dotnet build "Auctionhouse.Command.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctionhouse.Command.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["WebAPI/Auctionhouse.Command/entrypoint.sh", "WebAPI/wait-for", "./"] 
RUN chmod +x entrypoint.sh wait-for
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]