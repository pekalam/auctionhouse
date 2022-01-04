using Auctions.Domain;
using Auctions.Domain.Services;

namespace Auctionhouse.Command.Adapters
{
    internal class AuctionPaymentVerification : IAuctionPaymentVerification
    {
        public Task<bool> Verification(Auction auction, UserId buyer, string paymentMethod)
        {
            return Task.FromResult(true);
        }
    }
}
