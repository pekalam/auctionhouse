using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionUpdateEventGroup : UpdateEventGroup
    {
        public UserIdentity AuctionOwner { get; }

        public AuctionUpdateEventGroup(UserIdentity owner) : base("auctionUpdated")
        {
            AuctionOwner = owner;
        }

        public AuctionUpdateEventGroup(List<UpdateEvent> updateEvents, UserIdentity owner) : base("auctionUpdated", updateEvents)
        {
            AuctionOwner = owner;
        }
    }
}
