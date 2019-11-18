using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionTagsChanged : Event
    {
        public Guid AuctionId { get; }
        public Tag[] Tags { get; }

        public AuctionTagsChanged(Guid auctionId, Tag[] tags) : base(EventNames.AuctionTagsChanged)
        {
            AuctionId = auctionId;
            Tags = tags;
        }
    }
}
