using System;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCanceled : Event
    {
        public Guid AuctionId { get; }

        public AuctionCanceled(Guid auctionId) : base(EventsNames.AuctionCanceledEventName)
        {
            AuctionId = auctionId;
        }
    }
}
