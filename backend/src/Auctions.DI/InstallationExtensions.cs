using Adapter.AuctionImageConversion;
using Adapter.Dapper.AuctionhouseDatabase;
using Adapter.Hangfire_.Auctionhouse;
using Adapter.MongoDb;
using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IntegrationService.CategoryNamesToTreeIdsConversion;
using IntegrationService.AuctionPaymentVerification;

namespace Auctions.DI
{
    public static class InstallationExtensions
    {
        public static AuctionsModuleInstaller AddAuctionsModule(this IServiceCollection services, IConfiguration configuration)
        {
            var installer = new AuctionsModuleInstaller(services);
            installer
                .Domain
                    .AddDapperAuctionRepositoryAdapter(configuration)
                    .AddAuctionImageConversionAdapter()
                    .AddMongoDbImageRepositoryAdapter(configuration)
                    .AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(configuration)
                    .AddHangfireAuctionBuyCancellationSchedulerAdapter(configuration)
                    //INTEGRATION SERVICES
                    .AddCategoryNamesToTreeIdsConversionAdapter()
                    .AddAuctionPaymentVerificationAdapter();

            return installer;
        }
    }
}
