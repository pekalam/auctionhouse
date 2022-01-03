using Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionBids.Application
{
    public static class AuctionBidsInstaller
    {
        public static void AddAuctionBidsModule(this IServiceCollection services)
        {
            services.AddEventSubscribers(typeof(AuctionBidsInstaller));
        }
    }
}
