using Common.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionBids.Application
{
    public class AuctionBidsApplicationInstaller
    {
        public AuctionBidsApplicationInstaller(IServiceCollection services)
        {
            Services = services;
            services.AddEventSubscribers(typeof(AuctionBidsApplicationInstaller));
        }

        public IServiceCollection Services { get; }
    }
}
