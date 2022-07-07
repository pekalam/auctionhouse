using Auctions.DomainEvents;
using Core.Common.Domain;

namespace Auctions.DomainEvents.Update
{
    public class AuctionNameChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public string AuctionName { get; }

        public AuctionNameChanged(Guid auctionId, string auctionName) : base(EventNames.AuctionNameChanged)
        {
            AuctionId = auctionId;
            AuctionName = auctionName;
        }
    }
}
