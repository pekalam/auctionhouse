#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Auctionhouse.Command/Auctionhouse.Command.csproj", "Auctionhouse.Command/"]
COPY ["Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapter.EfCore.ReadModelNotifications/"]
COPY ["Common.Application/Common.Application.csproj", "Common.Application/"]
COPY ["Core.DomainFramework/Common.DomainFramework.csproj", "Core.DomainFramework/"]
COPY ["Adapter.SqlServer.EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapter.SqlServer.EventOutboxStorage/"]
COPY ["Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Auctions.Domain/Auctions.Domain.csproj", "Auctions.Domain/"]
COPY ["Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Auctions.Domain.Events/"]
COPY ["Adapter.XmlCategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Adapter.XmlCategoryTreeStore/"]
COPY ["Categories.Domain/Categories.Domain.csproj", "Categories.Domain/"]
COPY ["Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adatper.RabbitMq.EventBus/"]
COPY ["UserPayments.Application/UserPayments.Application.csproj", "UserPayments.Application/"]
COPY ["Users.DomainEvents/Users.DomainEvents.csproj", "Users.DomainEvents/"]
COPY ["UserPayments.Domain/UserPayments.Domain.csproj", "UserPayments.Domain/"]
COPY ["UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "UserPayments.DomainEvents/"]
COPY ["Adapter.QuartzTimeTaskService.AuctionEndScheduler/Adapter.QuartzTimeTaskService.AuctionEndScheduler.csproj", "Adapter.QuartzTimeTaskService.AuctionEndScheduler/"]
COPY ["Auctions.Application/Auctions.Application.csproj", "Auctions.Application/"]
COPY ["AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "AuctionBids.DomainEvents/"]
COPY ["IntegrationService.CategoryNamesToTreeIdsConversion/IntegrationService.CategoryNamesToTreeIdsConversion.csproj", "IntegrationService.CategoryNamesToTreeIdsConversion/"]
COPY ["Users.Application/Users.Application.csproj", "Users.Application/"]
COPY ["Users.Domain/Users.Domain.csproj", "Users.Domain/"]
COPY ["Common.WebAPI/Common.WebAPI.csproj", "Common.WebAPI/"]
COPY ["Adapter.AuctionImageConversion/Adapter.AuctionImageConversion.csproj", "Adapter.AuctionImageConversion/"]
COPY ["Adapter.Dapper.AuctionhouseDatabase/Adapter.Dapper.AuctionhouseDatabase.csproj", "Adapter.Dapper.AuctionhouseDatabase/"]
COPY ["AuctionBids.Domain/AuctionBids.Domain.csproj", "AuctionBids.Domain/"]
COPY ["AuctionBids.Application/AuctionBids.Application.csproj", "AuctionBids.Application/"]
COPY ["IntegrationService.AuctionPaymentVerification/IntegrationService.AuctionPaymentVerification.csproj", "IntegrationService.AuctionPaymentVerification/"]
COPY ["Adapter.Hangfire.Auctionhouse/Adapter.Hangfire_.Auctionhouse.csproj", "Adapter.Hangfire.Auctionhouse/"]
RUN dotnet restore "Auctionhouse.Command/Auctionhouse.Command.csproj"
COPY . .
WORKDIR "/src/Auctionhouse.Command"
RUN dotnet build "Auctionhouse.Command.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctionhouse.Command.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY ["Auctionhouse.Command/entrypoint.sh", "wait-for", "."] 
RUN chmod +x entrypoint.sh wait-for
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]