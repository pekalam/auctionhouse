using Auctions.DomainEvents;
using Core.Common.Domain;

namespace Auctions.DomainEvents.Update
{
    public class AuctionCategoriesChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public int[] Categories { get; }

        public AuctionCategoriesChanged(Guid auctionId, int[] categories) : base(EventNames.AuctionCategoriesChanged)
        {
            AuctionId = auctionId;
            Categories = categories;
        }
    }
}
