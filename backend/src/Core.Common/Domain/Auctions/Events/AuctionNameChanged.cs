using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionNameChanged : Event
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
