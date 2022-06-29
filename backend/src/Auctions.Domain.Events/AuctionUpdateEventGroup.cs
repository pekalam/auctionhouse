using Core.Common.Domain;

namespace Auctions.DomainEvents
{
    public class AuctionUpdateEventGroup : UpdateEventGroup
    {
        public Guid AuctionOwner { get; set; }
        public CategoryIds CategoryIds { get; set; } = null!;

        public AuctionUpdateEventGroup(Guid owner) : base(EventNames.AuctionUpdated)
        {
            AuctionOwner = owner;
        }
    }
}
