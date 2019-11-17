using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Bids;

namespace Core.Common.Domain.Auctions.Events
{
    public class BidRemoved : Event
    {
        public Bid Bid { get; set; }

        public BidRemoved(Bid bid) : base(EventNames.BidRemoved)
        {
            Bid = bid;
        }
    }
}
