using AuctionBids.Application;
using AuctionBids.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionBids.DI
{
    public class AuctionBidsModuleInstaller
    {
        public AuctionBidsModuleInstaller(IServiceCollection services)
        {
            Application = new AuctionBidsApplicationInstaller(services);
            Domain = new AuctionBidsDomainInstaller(services);
        }

        public AuctionBidsApplicationInstaller Application { get; private set; }
        public AuctionBidsDomainInstaller Domain { get; private set; }
    }
}