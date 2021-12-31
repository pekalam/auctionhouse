using Core.Common.Domain;

namespace Auctions.Domain.Events
{
    public class AuctionCreated : Event
    {
        public Guid AuctionId { get; }
        public AuctionArgs AuctionArgs { get; }

        public AuctionCreated(Guid auctionId, AuctionArgs auctionArgs) : base(EventNames.AuctionCreated)
        {
            AuctionId = auctionId;
            AuctionArgs = auctionArgs;
        }
    }
}