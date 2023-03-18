using Adapter.Dapper.AuctionhouseDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionBids.DI
{
    public static class InstallationExtensions
    {
        public static void AddAuctionBidsModule(this IServiceCollection services, IConfiguration configuration)
        {
            new AuctionBidsModuleInstaller(services)
                .Domain
                    .AddDapperAuctionBidsRepositoryAdapter(configuration);
        }
    }
}