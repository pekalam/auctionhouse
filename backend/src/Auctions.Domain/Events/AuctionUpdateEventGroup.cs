using Core.Common.Domain;
using Core.Common.Domain.Auctions;

namespace Auctions.Domain.Events
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
