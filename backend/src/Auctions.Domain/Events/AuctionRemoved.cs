using Core.Common.Domain;

namespace Auctions.Domain.Events
{
    public class AuctionRemoved : Event
    {
        public Guid AuctionId { get; }

        public AuctionRemoved(Guid auctionId) : base(EventNames.AuctionRemoved)
        {
            AuctionId = auctionId;
        }
    }
}
