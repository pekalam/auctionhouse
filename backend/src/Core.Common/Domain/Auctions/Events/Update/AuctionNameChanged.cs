using System;

namespace Core.Common.Domain.Auctions.Events.Update
{
    public class AuctionNameChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public AuctionName AuctionName { get; }

        public AuctionNameChanged(Guid auctionId, AuctionName auctionName) : base(EventNames.AuctionNameChanged)
        {
            AuctionId = auctionId;
            AuctionName = auctionName;
        }
    }
}
