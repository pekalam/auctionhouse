using Auctions.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Adapter.AuctionImageConversion
{
    public static class AuctionImageConversionInstaller
    {
        public static void AddAuctionImageConversion(this IServiceCollection services)
        {
            services.AddTransient<IAuctionImageConversion, AuctionImageConversionService>();
        }
    }
}