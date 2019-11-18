using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Categories;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCategoryChanged : Event
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
