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
COPY ["Modules/Auctions/Auctions.Application/Auctions.Application.csproj", "Modules/Auctions/Auctions.Application/"]
COPY ["Modules/Auctions/Auctions.Domain/Auctions.Domain.csproj", "Modules/Auctions/Auctions.Domain/"]
COPY ["Modules/Auctions/Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Modules/Auctions/Auctions.Domain.Events/"]
COPY ["Modules/Common/Core.DomainFramework/Common.DomainFramework.csproj", "Modules/Common/Core.DomainFramework/"]
COPY ["Modules/Common/Common.Application/Common.Application.csproj", "Modules/Common/Common.Application/"]
COPY ["Modules/UserPayments/UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "Modules/UserPayments/UserPayments.DomainEvents/"]
COPY ["Modules/AuctionBids/AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "Modules/AuctionBids/AuctionBids.DomainEvents/"]
COPY ["Modules/Users/Users.Application/Users.Application.csproj", "Modules/Users/Users.Application/"]
COPY ["Modules/Users/Users.Domain/Users.Domain.csproj", "Modules/Users/Users.Domain/"]
COPY ["Modules/Users/Users.DomainEvents/Users.DomainEvents.csproj", "Modules/Users/Users.DomainEvents/"]
COPY ["Modules/UserPayments/UserPayments.Domain/UserPayments.Domain.csproj", "Modules/UserPayments/UserPayments.Domain/"]
COPY ["Adapters/Adapter.ChronicleEfCoreStorage/ChronicleEfCoreStorage.csproj", "Adapters/Adapter.ChronicleEfCoreStorage/"]
COPY ["Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/"]
COPY ["Modules/IntegrationServices/IntegrationService.CategoryNamesToTreeIdsConversion/IntegrationService.CategoryNamesToTreeIdsConversion.csproj", "Modules/IntegrationServices/IntegrationService.CategoryNamesToTreeIdsConversion/"]
COPY ["Modules/Categories/Categories.Domain/Categories.Domain.csproj", "Modules/Categories/Categories.Domain/"]
COPY ["Modules/UserPayments/UserPayments.Application/UserPayments.Application.csproj", "Modules/UserPayments/UserPayments.Application/"]
COPY ["Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Adapters/Schedulers/Adapter.QuartzTimeTaskService.AuctionEndScheduler/Adapter.QuartzTimeTaskService.AuctionEndScheduler.csproj", "Adapters/Schedulers/Adapter.QuartzTimeTaskService.AuctionEndScheduler/"]
COPY ["Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/"]
COPY ["Adapters/AuctionImageConversion/Adapter.AuctionImageConversion/Adapter.AuctionImageConversion.csproj", "Adapters/AuctionImageConversion/Adapter.AuctionImageConversion/"]
COPY ["Modules/IntegrationServices/IntegrationService.AuctionPaymentVerification/IntegrationService.AuctionPaymentVerification.csproj", "Modules/IntegrationServices/IntegrationService.AuctionPaymentVerification/"]
COPY ["Modules/AuctionBids/AuctionBids.Domain/AuctionBids.Domain.csproj", "Modules/AuctionBids/AuctionBids.Domain/"]
COPY ["Adapters/EventBus/Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adapters/EventBus/Adatper.RabbitMq.EventBus/"]
COPY ["WebAPI/Common.WebAPI/WebAPI.Common.csproj", "WebAPI/Common.WebAPI/"]
COPY ["Modules/AuctionBids/AuctionBids.Application/AuctionBids.Application.csproj", "Modules/AuctionBids/AuctionBids.Application/"]
COPY ["Adapters/Repositories/Adapter.Dapper.AuctionhouseDatabase/Adapter.Dapper.AuctionhouseDatabase.csproj", "Adapters/Repositories/Adapter.Dapper.AuctionhouseDatabase/"]
COPY ["Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/"]
COPY ["Adapters/Schedulers/Adapter.Hangfire.Auctionhouse/Adapter.Hangfire_.Auctionhouse.csproj", "Adapters/Schedulers/Adapter.Hangfire.Auctionhouse/"]
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