using Core.Common.Domain;

namespace AuctionBids.Domain.DomainEvents
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
