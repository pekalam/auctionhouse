using Core.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Domain.Auctions.Services
{
    public interface IAuctionPaymentVerification
    {
        Task<bool> Verification(Auction auction, UserId buyer, string paymentMethod);
    }

    public interface IAuctionUnlockSheduler
    {
        Task SheduleUnlock(Auction auction);
    }

    public class AuctionUnlockService //unlock method is internal in order to not call it accidentaly in other way than through than dedicated service
    {
        private IAuctionRepository _auctions;

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

            auction.Unlock();
        }
    }
}
