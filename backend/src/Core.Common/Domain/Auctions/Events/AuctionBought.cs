using System;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionBought : Event
    {
        public Guid AuctionId { get; }
        public UserIdentity UserIdentity { get; }
        public UserIdentity AuctionOwner { get; }

        public AuctionBought(Guid auctionId, UserIdentity userIdentity,
            UserIdentity auctionOwner) : base(EventNames.AuctionBought)
        {
            AuctionId = auctionId;
            UserIdentity = userIdentity;
            AuctionOwner = auctionOwner;
        }
    }
}