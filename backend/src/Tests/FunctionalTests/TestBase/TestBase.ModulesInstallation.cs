using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.DI;
using Auctionhouse.Command.Adapters;
using Auctions.Application;
using Auctions.DI;
using Auctions.Domain;
using Auctions.Tests.Base;
using Categories.Domain;
using ChronicleEfCoreStorage;
using Common.Application.Events;
using Common.Application.DependencyInjection;
using Common.Tests.Base;
using Common.Tests.Base.Mocks;
using FunctionalTests.Mocks;
using IntegrationService.AuctionPaymentVerification;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using ReadModel.Core;
using ReadModel.Core.Services;
using System;
using TestConfigurationAccessor;
using UserPayments.DI;
using Users.DI;
using XmlCategoryTreeStore;
using Auctions.Application.Sagas;
using Users.Application.Sagas;

namespace FunctionalTests.Commands
{
    public partial class TestBase
    {
        private IServiceProvider BuildConfiguredServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton(TestConfig.Instance);

            SetupCommonModule(services);

            SetupAuctions(services);

            SetupCategories(services);

            SetupAuctionBids(services);

            SetupUsers(services);

            SetupUserPayments(services);

            SetupReadModel(services);

            // decorate event bus to be able view sent messages
            services.Decorate<IEventBus, InMemoryEventBusDecorator>();
            // adding httpcontext accessor with singleton session in order to satisfy AuctionCreateSessionStore requirements
            services.AddScoped<IHttpContextAccessor, TestHttpContextAccessor>();

            services.AddChronicleSQLServerStorage(TestConfig.Instance.GetChronicleSQLServerStorageConnectionString());

            services.AddCommandEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());
            services.AddQueryEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());

            services.AddLogging(c =>
            {
                c.AddXUnit(_outputHelper);
                c.SetMinimumLevel(LogLevel.Debug);
            });


            AddCustomServices(services);

            return services.BuildServiceProvider();
        }

        private static void SetupReadModel(ServiceCollection services)
        {
            new ReadModelInstaller(services, TestConfig.Instance, "FunctionalTests:ReadModelSettings")
                .AddBidRaisedNotifications(_ => Mock.Of<IBidRaisedNotifications>());
        }

        private static void SetupUserPayments(ServiceCollection services)
        {
            new UserPaymentsModuleInstaller(services)
                .Domain
                    .AddDapperUserPaymentsRepositoryAdapter(settings: TestConfig.Instance.GetAuctionhouseRepositorySettings());
        }

        private static void SetupUsers(ServiceCollection services)
        {
            new UsersModuleInstaller(services)
                .Domain
                    .AddDapperUserRepositoryAdapter(settings: TestConfig.Instance.GetAuctionhouseRepositorySettings())
                    .AddDapperUserAuthenticationDataRepositoryAdapter(settings: TestConfig.Instance.GetAuctionhouseRepositorySettings());
        }

        private static void SetupAuctionBids(ServiceCollection services)
        {
            new AuctionBidsModuleInstaller(services)
                .Domain
                 .AddDapperAuctionBidsRepositoryAdapter(settings: TestConfig.Instance.GetAuctionhouseRepositorySettings());
        }

        private static void SetupCategories(ServiceCollection services)
        {
            new CategoriesInstaller(services)
                .AddXmlCategoryTreeStoreAdapter(settings: TestConfig.Instance.GetXmlStoreSettings());
        }

        private void SetupCommonModule(ServiceCollection services)
        {
            //missing query dependencies
            var common = new CommonApplicationMockInstaller(services)
                .AddCommandCoreDependencies(
                    null,
                       ImplProviderMock.Factory,
                       assemblies)
                .AddQueryCoreDependencies(
                    implProviderFactory: ImplProviderMock.Factory,
                    assemblies
                )
                .AddRabbitMqEventBusAdapter(null, rabbitMqSettings: TestConfig.Instance.GetRabbitMqSettings())
                .AddRabbitMqAppEventBuilderAdapter()
                .AddOutboxItemStore(_ => InMemoryOutboxItemStore.Create())
                .AddOutboxItemFinder(_ => InMemoryOutboxItemFinder.Create())
                .AddUserIdentityService(_ => _userIdentityService.Object);
            ConfigureCommonModuleCustomDependencies(common);
        }

        protected virtual void ConfigureCommonModuleCustomDependencies(CommonApplicationInstaller installer)
        {

        }

        private void SetupAuctions(ServiceCollection services)
        {
            var auctions = new AuctionsMockInstaller(services);
            auctions.Domain
                .AddAuctionCreateSessionStoreAdapter()
                .AddDapperAuctionRepositoryAdapter(settings: TestConfig.Instance.GetAuctionhouseRepositorySettings())
                .AddCategoryNamesToTreeIdsConversion((prov) => ConvertCategoryNamesToRootToLeafIdsMock.Create())
                .AddAuctionEndScheduler((prov) => AuctionEndSchedulerMock.Create())
                .AddAuctionPaymentVerificationAdapter();
            ConfigureAuctionsModuleCustomDependencies(auctions);
        }

        protected virtual void ConfigureAuctionsModuleCustomDependencies(AuctionsModuleInstaller installer)
        {

        }

        protected virtual void AddCustomServices(IServiceCollection services)
        {

        }
    }
}
