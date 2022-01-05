using Auctions.Domain;
using System;

namespace Test.Auctions.Domain
{
    public class GivenAuction
    {
        public Auction ValidOfTypeBuyNowAndBid()
        {
            return new Auction(new GivenAuctionArgs().ValidBuyNowAndBid());
        }
    }
}
