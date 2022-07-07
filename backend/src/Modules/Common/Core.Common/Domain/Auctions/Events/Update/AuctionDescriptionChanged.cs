using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events.Update
{
    public class AuctionDescriptionChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public string Description { get; }

        public AuctionDescriptionChanged(Guid auctionId, string description) : base(EventNames.AuctionDescriptionChanged)
        {
            AuctionId = auctionId;
            Description = description;
        }
    }
}
