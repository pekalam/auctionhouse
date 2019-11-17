using System;
using Core.Common.Domain.Bids;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCompleted : Event
    {
        public Guid AuctionId { get; }
        public Bid WinningBid { get; }

        public AuctionCompleted(Guid auctionId, Bid winningBid) : base(EventNames.AuctionCompletedEventName)
        {
            AuctionId = auctionId;
            WinningBid = winningBid;
        }
    }
}