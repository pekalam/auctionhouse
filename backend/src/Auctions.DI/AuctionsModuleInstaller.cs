using Auctions.Application.DependencyInjection;
using Auctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.DI
{
    public class AuctionsModuleInstaller
    {
        public AuctionsModuleInstaller(IServiceCollection services)
        {
            Domain = new AuctionsDomainInstaller(services);
            Application = new AuctionsApplicationInstaller(services);
        }

        public virtual AuctionsDomainInstaller Domain { get; }
        public virtual AuctionsApplicationInstaller Application { get; }
    }

}