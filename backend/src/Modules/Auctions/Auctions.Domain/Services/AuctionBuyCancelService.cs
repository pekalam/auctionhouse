using Auctions.Domain.Repositories;

namespace Auctions.Domain.Services
{
    public class AuctionBuyCancellationService //cancel method is internal in order to not call it accidentally in other way than through than dedicated service
    {
        public Auction? Cancel(AuctionId auctionId, IAuctionRepository auctions)
        {
            var auction = auctions.FindAuction(auctionId);
            if (auction is null)
            {
                return null;
            }
            auction.CancelBuy();

            return auction;
        }
    }
}
