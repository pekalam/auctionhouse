using System;

namespace Core.Common.Domain.AuctionBids
{
    public class AuctionCancelled : Event
    {
        public Guid AuctionId { get; set; }
        public Guid WinnerId { get; set; }
        public decimal CurrentPrice { get; set; }

        public AuctionCancelled() : base("auctionCancelled")
        {
        }
    }
}
