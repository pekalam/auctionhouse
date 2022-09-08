using Auctions.Application;
using Auctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.DI
{
    public class AuctionsInstaller
    {
        private readonly AuctionsDomainInstaller _domainInstaller;
        private readonly AuctionsApplicationInstaller _applicationInstaller;

        public AuctionsInstaller(IServiceCollection services)
        {
            _domainInstaller = new AuctionsDomainInstaller(services);
            _applicationInstaller = new AuctionsApplicationInstaller(services);
        }

        public AuctionsDomainInstaller Domain => _domainInstaller;
        public AuctionsApplicationInstaller Application => _applicationInstaller;
    }

}