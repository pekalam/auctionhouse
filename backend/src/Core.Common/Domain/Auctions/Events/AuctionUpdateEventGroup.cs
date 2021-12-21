using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionUpdateEventGroup : UpdateEventGroup<AuctionId>
    {
        public UserIdentity AuctionOwner { get; set; }

        public AuctionUpdateEventGroup(UserIdentity owner) : base(EventNames.AuctionUpdated)
        {
            AuctionOwner = owner;
        }
    }
}
