using Auctions.Application;
using Auctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.DI
{
    public class AuctionsInstaller
    {
        public AuctionsInstaller(IServiceCollection services)
        {
            Domain = new AuctionsDomainInstaller(services);
            Application = new AuctionsApplicationInstaller(services);
        }

        public virtual AuctionsDomainInstaller Domain { get; }
        public virtual AuctionsApplicationInstaller Application { get; }
    }

}