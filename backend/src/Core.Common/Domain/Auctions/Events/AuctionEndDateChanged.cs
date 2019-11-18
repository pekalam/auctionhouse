using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionEndDateChanged : Event
    {
        public Guid AuctionId { get; }
        public AuctionDate Date { get; }

        public AuctionEndDateChanged(Guid auctionId, AuctionDate date) : base(EventNames.AuctionEndDateChanged)
        {
            AuctionId = auctionId;
            Date = date;
        }
    }
}
