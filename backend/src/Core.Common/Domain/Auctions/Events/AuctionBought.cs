using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionBought : Event
    {
        public Guid AuctionId { get; }
        public Guid UserIdentity { get; }
        public Guid AuctionOwner { get; }

        public AuctionBought(Guid auctionId, Guid userIdentity,
            Guid auctionOwner) : base(EventNames.AuctionBought)
        {
            AuctionId = auctionId;
            UserIdentity = userIdentity;
            AuctionOwner = auctionOwner;
        }
    }
}