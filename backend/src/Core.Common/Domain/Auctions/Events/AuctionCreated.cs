using System;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionCreated : Event
    {
        public Guid AuctionId { get; }
        public AuctionArgs AuctionArgs { get; }

        public AuctionCreated(Guid auctionId, AuctionArgs auctionArgs) : base(EventNames.AuctionCreatedEventName)
        {
            AuctionId = auctionId;
            AuctionArgs = auctionArgs;
        }
    }
}