using Auctions.Application.Commands.CreateAuction;
using Auctions.DomainEvents;
using Chronicle;
using Microsoft.Extensions.DependencyInjection;
using static AuctionBids.DomainEvents.Events.V1;

namespace Auctions.Application
{
    public static class AuctionsInstaller
    {
        public static void AddAuctionsModule(this IServiceCollection services)
        {
            services.AddChronicle(build =>
            {
                build.UseInMemoryPersistence();
            });
        }
    }
}
