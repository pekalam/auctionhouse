using Core.Common.Domain;

namespace AuctionBids.Domain.DomainEvents
{
    public class AuctionPriceRised : Event
    {
        public Guid AuctionBidsId { get; set; }
        public Guid BidId { get; set; }
        public Guid AuctionId { get; set; }
        public Guid WinnerId { get; set; }
        public DateTime Date { get; set; }
        public decimal CurrentPrice { get; set; }

        public AuctionPriceRised() : base("auctionPriceRised")
        {
        }
    }

    public class AuctionHaveNoParticipants : Event
    {
        public Guid AuctionBidsId { get; set; }
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; }

        public AuctionHaveNoParticipants() : base("auctionNoParticipants")
        {
        }
    }

    public class AuctionBidNotAccepted : Event
    {
        public Guid AuctionBidsId { get; set; }
        public Guid BidId { get; set; } //TODO internal events?
        public Guid AuctionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }

        public AuctionBidNotAccepted() : base("auctionBidNotAccepted")
        {
        }
    }
}
