using Core.Common.Domain;

namespace Auctions.Domain.Events.Update
{
    public class AuctionCategoryChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public int Category { get; }

        public AuctionCategoryChanged(Guid auctionId, int category) : base(EventNames.AuctionCategoryChanged)
        {
            AuctionId = auctionId;
            Category = category;
        }
    }
}
