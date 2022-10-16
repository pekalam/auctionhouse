using AuctionBids.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionBids.Domain
{
    public class AuctionBidsDomainInstaller
    {
        public AuctionBidsDomainInstaller(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public AuctionBidsDomainInstaller AddAuctionBidsRepository(Func<IServiceProvider, IAuctionBidsRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
