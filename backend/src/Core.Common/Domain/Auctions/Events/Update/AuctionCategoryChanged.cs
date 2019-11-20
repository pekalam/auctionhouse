using System;
using Core.Common.Domain.Categories;

namespace Core.Common.Domain.Auctions.Events.Update
{
    public class AuctionCategoryChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public Category Category { get; }

        public AuctionCategoryChanged(Guid auctionId, Category category) : base(EventNames.AuctionCategoryChanged)
        {
            AuctionId = auctionId;
            Category = category;
        }
    }
}
