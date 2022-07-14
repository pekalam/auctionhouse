FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
RUN apt-get update && apt-get install -y libsnappy-dev && apt-get install -y libc6-dev # rocksdb requirements
RUN apt-get install -y ncat
COPY ["Tests/FunctionalTests/wait-for", "/bin/wait-for"]
RUN chmod +x /bin/wait-for
COPY ["Tests/FunctionalTests/entrypoint.sh", "/bin/entrypoint.sh"]
RUN chmod +x /bin/entrypoint.sh

WORKDIR /src
COPY ["Tests/FunctionalTests/FunctionalTests.csproj", "Tests/FunctionalTests/"]
COPY ["Adapters/Adapter.ChronicleEfCoreStorage/ChronicleEfCoreStorage.csproj", "Adapters/Adapter.ChronicleEfCoreStorage/"]
COPY ["Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/Adapter.EfCore.ReadModelNotifications.csproj", "Adapters/ReadModelNotifications/Adapter.EfCore.ReadModelNotifications/"]
COPY ["Modules/Common/Common.Application/Common.Application.csproj", "Modules/Common/Common.Application/"]
COPY ["Modules/Common/Core.DomainFramework/Common.DomainFramework.csproj", "Modules/Common/Core.DomainFramework/"]
COPY ["Modules/IntegrationServices/IntegrationService.AuctionPaymentVerification/IntegrationService.AuctionPaymentVerification.csproj", "Modules/IntegrationServices/IntegrationService.AuctionPaymentVerification/"]
COPY ["Modules/AuctionBids/AuctionBids.Domain/AuctionBids.Domain.csproj", "Modules/AuctionBids/AuctionBids.Domain/"]
COPY ["Modules/AuctionBids/AuctionBids.DomainEvents/AuctionBids.DomainEvents.csproj", "Modules/AuctionBids/AuctionBids.DomainEvents/"]
COPY ["Modules/Auctions/Auctions.Domain/Auctions.Domain.csproj", "Modules/Auctions/Auctions.Domain/"]
COPY ["Modules/Auctions/Auctions.Domain.Events/Auctions.DomainEvents.csproj", "Modules/Auctions/Auctions.Domain.Events/"]
COPY ["Modules/UserPayments/UserPayments.Domain/UserPayments.Domain.csproj", "Modules/UserPayments/UserPayments.Domain/"]
COPY ["Modules/UserPayments/UserPayments.DomainEvents/UserPayments.DomainEvents.csproj", "Modules/UserPayments/UserPayments.DomainEvents/"]
COPY ["Modules/Users/Users.Domain/Users.Domain.csproj", "Modules/Users/Users.Domain/"]
COPY ["Modules/Users/Users.DomainEvents/Users.DomainEvents.csproj", "Modules/Users/Users.DomainEvents/"]
COPY ["Modules/AuctionBids/AuctionBids.Application/AuctionBids.Application.csproj", "Modules/AuctionBids/AuctionBids.Application/"]
COPY ["TestConfigurationAccessor/TestConfigurationAccessor.csproj", "TestConfigurationAccessor/"]
COPY ["WebAPI/Auctionhouse.Query/Auctionhouse.Query.csproj", "WebAPI/Auctionhouse.Query/"]
COPY ["Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository.csproj", "Adapters/AuctionImageRepository/Adapter.MongoDb.AuctionImageRepository/"]
COPY ["Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage.csproj", "Adapters/EventOutboxStorage/Adapter.SqlServer.EventOutboxStorage/"]
COPY ["Modules/Categories/Categories.Domain/Categories.Domain.csproj", "Modules/Categories/Categories.Domain/"]
COPY ["Adapters/EventBus/Adatper.RabbitMq.EventBus/Adatper.RabbitMq.EventBus.csproj", "Adapters/EventBus/Adatper.RabbitMq.EventBus/"]
COPY ["Modules/ReadModel/ReadModel.Core/ReadModel.Core.csproj", "Modules/ReadModel/ReadModel.Core/"]
COPY ["Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/Adapter.XmlCategoryTreeStore.csproj", "Adapters/CategoryStore/Adapter.XmlCategoryTreeStore/"]
COPY ["WebAPI/Common.WebAPI/Common.WebAPI.csproj", "WebAPI/Common.WebAPI/"]
COPY ["Modules/Auctions/Auctions.Application/Auctions.Application.csproj", "Modules/Auctions/Auctions.Application/"]
COPY ["Modules/Auctions/Tests/Test.Auctions.Base/Auctions.Tests.Base.csproj", "Modules/Auctions/Tests/Test.Auctions.Base/"]
COPY ["Modules/ReadModel/Tests/Test.ReadModel.Base/ReadModel.Tests.Base.csproj", "Modules/ReadModel/Tests/Test.ReadModel.Base/"]
COPY ["Modules/Users/Tests/Test.Users.Base/Users.Tests.Base.csproj", "Modules/Users/Tests/Test.Users.Base/"]
COPY ["Adapters/CategoryStore/Test.XmlCategoryTreeStore/Test.XmlCategoryTreeStore.csproj", "Adapters/CategoryStore/Test.XmlCategoryTreeStore/"]
COPY ["Modules/UserPayments/UserPayments.Application/UserPayments.Application.csproj", "Modules/UserPayments/UserPayments.Application/"]
COPY ["Modules/Users/Users.Application/Users.Application.csproj", "Modules/Users/Users.Application/"]
RUN dotnet restore "Tests/FunctionalTests/FunctionalTests.csproj"
COPY . .
WORKDIR "Tests/FunctionalTests"

ENTRYPOINT [ "entrypoint.sh" ]