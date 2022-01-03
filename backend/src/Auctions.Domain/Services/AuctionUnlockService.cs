using Auctions.Domain.Repositories;
using Core.DomainFramework;

namespace Auctions.Domain.Services
{
    public class AuctionUnlockService //unlock method is internal in order to not call it accidentaly in other way than through than dedicated service
    {
        private readonly IAuctionRepository _auctions;

        public AuctionUnlockService(IAuctionRepository auctions)
        {
            _auctions = auctions;
        }

        public void Unlock(AuctionId auctionId)
        {
            var auction = _auctions.FindAuction(auctionId);
            if (auction == null)
            {
                throw new DomainException("Could not find auction");
            }

            auction.Unlock(auction.LockIssuer);
        }
    }
}
