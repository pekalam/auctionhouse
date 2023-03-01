using Adapter.AuctionImageConversion;
using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.Hangfire_.Auctionhouse;
using Adapter.MongoDb;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Auctionhouse.Command.Adapters;
using Auctions.DI;
using Common.Application.Events;
using Common.Application;
using IntegrationService.AuctionPaymentVerification;
using IntegrationService.CategoryNamesToTreeIdsConversion;
using System.Reflection;
using static System.Convert;
using Adapter.SqlServer.EventOutbox;
using RabbitMq.EventBus;
using Categories.DI;
using XmlCategoryTreeStore;
using AuctionBids.DI;
using Users.DI;
using UserPayments.DI;
using Common.Application.DependencyInjection;

namespace Auctionhouse.Command.ModuleInstallation
{
    internal static class InstallationExtensions
    {
        public static void AddAuctionsModule(this IServiceCollection services, IConfiguration configuration)
        {
            new AuctionsInstaller(services)
                .Domain
                    .AddDapperAuctionRepositoryAdapter(configuration)
                    .AddAuctionImageConversionAdapter()
                    .AddMongoDbImageRepositoryAdapter(configuration)
                    .AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(configuration)
                    .AddHangfireAuctionUnlockSchedulerAdapter(configuration)
                    .AddAuctionCreateSessionStoreAdapter()
                    //INTEGRATION SERVICES
                    .AddCategoryNamesToTreeIdsConversionAdapter()
                    .AddAuctionPaymentVerificationAdapter();
            new AuctionsInstaller(services)
                .Application
                    .AddTempFileServiceAdapter();
        }

        public static void AddCommonModule(this IServiceCollection services, IConfiguration configuration, Assembly[] modules)
        {
            new CommonApplicationInstaller(services)
                .AddCommandCoreDependencies(modules)
                //OUTBOX PROCESSOR BG SERVICE
                .AddEventOutbox(new EventOutboxProcessorSettings
                {
                    MinMilisecondsDiff = ToInt32(configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.MinMilisecondsDiff)]),
                    EnableLogging = ToBoolean(configuration.GetSection(nameof(EventOutboxProcessorSettings))[nameof(EventOutboxProcessorSettings.EnableLogging)]),
                })
                .AddSqlServerEventOutboxStorageAdapter(configuration)
                .AddRabbitMqAppEventBuilderAdapter()
                .AddRabbitMqEventBusAdapter(configuration, eventSubscriptionAssemblies: modules);
        }

        public static void AddCategoriesModule(this IServiceCollection services, IConfiguration configuration)
        {
            new CategoriesInstaller(services)
                .AddXmlCategoryTreeStoreAdapter(configuration);
        }

        public static void AddAuctionBidsModule(this IServiceCollection services, IConfiguration configuration)
        {
            new AuctionBidsInstaller(services)
                .Domain
                    .AddDapperAuctionBidsRepositoryAdapter(configuration);
        }

        public static void AddUsersModule(this IServiceCollection services, IConfiguration configuration)
        {
            new UsersInstaller(services)
                .Domain
                    .AddDapperResetPasswordCodeRepositoryAdapter(configuration)
                    .AddDapperUserRepositoryAdapter(configuration)
                    .AddDapperUserAuthenticationDataRepositoryAdapter(configuration)
                    .AddResetLinkSenderServiceAdapter();
        }

        public static void AddUserPaymentsModule(this IServiceCollection services, IConfiguration configuration)
        {
            new UserPaymentsInstaller(services)
                .Domain
                    .AddDapperUserPaymentsRepositoryAdapter(configuration);
        }
    }
}
