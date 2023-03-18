using Auctions.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationService.AuctionPaymentVerification
{
    public static class AuctionPaymentVerificationInstaller
    {
        public static AuctionsDomainInstaller AddAuctionPaymentVerificationAdapter(this AuctionsDomainInstaller installer)
        {
            installer.Services.AddTransient<AuctionPaymentVerification>();

            installer.AddAuctionPaymentVerification((prov) => prov.GetRequiredService<AuctionPaymentVerification>());
            return installer;
        }

    }
}