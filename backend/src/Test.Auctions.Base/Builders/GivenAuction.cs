using Auctions.Domain;
using System;

namespace Auctions.Tests.Base.Builders
{
    public class GivenAuction
    {
        private AuctionArgs _args = new GivenAuctionArgs().ValidBuyNowAndBid();
        private AuctionBidsId? _auctionBidsId;

        public GivenAuction WithAuctionArgs(AuctionArgs auctionArgs)
        {
            _args = auctionArgs;
            return this;
        }

        public GivenAuction WithAssignedAuctionBidsId(AuctionBidsId? auctionBidsId = null)
        {
            _auctionBidsId = auctionBidsId;
            return this;
        }

        public Auction Build()
        {
            var auction = new Auction(_args);
            if (_auctionBidsId is not null)
            {
                auction.AddAuctionBids(_auctionBidsId);
            }
            return auction;
        }

        public Auction ValidOfTypeBuyNowAndBid(AuctionBidsId? auctionBidsId = null)
        {
            _args = new GivenAuctionArgs().ValidBuyNowAndBid();
            _auctionBidsId = auctionBidsId ?? new AuctionBidsId(Guid.NewGuid());
            WithAssignedAuctionBidsId(_auctionBidsId);
            return Build();
        }
    }
}
