using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionRemoved : AuctionEvent
    {
        public Guid AuctionId { get; }

        public AuctionRemoved(Guid auctionId) : base(EventNames.AuctionRemoved)
        {
            AuctionId = auctionId;
        }
    }
}
