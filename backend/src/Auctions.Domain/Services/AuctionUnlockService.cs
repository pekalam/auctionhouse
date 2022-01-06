using Auctions.Domain.Repositories;
using Core.DomainFramework;

namespace Auctions.Domain.Services
{
    public class AuctionUnlockService //unlock method is internal in order to not call it accidentaly in other way than through than dedicated service
    {
        public Auction Unlock(AuctionId auctionId, IAuctionRepository auctions)
        {
            var auction = auctions.FindAuction(auctionId);
            if (auction == null)
            {
                throw new DomainException("Could not find auction");
            }

            auction.Unlock(auction.LockIssuer);
            return auction;
        }
    }
}
