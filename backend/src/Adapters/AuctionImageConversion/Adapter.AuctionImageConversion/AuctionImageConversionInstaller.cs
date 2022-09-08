using Auctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.AuctionImageConversion
{
    public static class AuctionImageConversionInstaller
    {
        public static AuctionsDomainInstaller AddAuctionImageConversionAdapter(this AuctionsDomainInstaller installer)
        {
            installer.Services.AddTransient<AuctionImageConversionService>();
            installer.AddAuctionImageConversion((prov) => prov.GetRequiredService<AuctionImageConversionService>());
            return installer;
        }
    }
}