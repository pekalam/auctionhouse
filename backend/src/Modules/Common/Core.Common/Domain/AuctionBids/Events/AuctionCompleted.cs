using System;

namespace Core.Common.Domain.AuctionBids
{
    public class AuctionCompleted : Event
    {
        public Guid AuctionBidsId { get; set; }
        public Guid AuctionId { get; set; }
        public Guid WinnerId { get; set; }
        public decimal CurrentPrice { get; set; }

        public AuctionCompleted() : base("auctionCompleted")
        {
        }
    }
}
