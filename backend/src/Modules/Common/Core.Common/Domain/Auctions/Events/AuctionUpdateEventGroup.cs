using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionUpdateEventGroup : UpdateEventGroup<AuctionId>
    {
        public Guid AuctionOwner { get; set; }

        public AuctionUpdateEventGroup(Guid owner) : base(EventNames.AuctionUpdated)
        {
            AuctionOwner = owner;
        }
    }
}
