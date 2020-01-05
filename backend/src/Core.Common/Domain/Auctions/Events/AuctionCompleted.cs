using System;
using Core.Common.Domain.Bids;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCompleted : Event
    {
        public Guid AuctionId { get; }
        public Bid WinningBid { get; }
        public UserIdentity AuctionOwner { get; }

        public AuctionCompleted(Guid auctionId, Bid winningBid, UserIdentity auctionOwner) : base(EventNames.AuctionCompleted)
        {
            AuctionId = auctionId;
            WinningBid = winningBid;
            AuctionOwner = auctionOwner;
        }
    }
}