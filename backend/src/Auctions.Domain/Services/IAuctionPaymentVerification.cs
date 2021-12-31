using Core.Common.Domain.Auctions;

namespace Auctions.Domain.Services
{
    public interface IAuctionPaymentVerification
    {
        Task<bool> Verification(Auction auction, UserId buyer, string paymentMethod);
    }
}
