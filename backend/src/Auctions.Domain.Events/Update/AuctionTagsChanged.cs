using Auctions.DomainEvents;
using Core.Common.Domain;

namespace Auctions.DomainEvents.Update
{
    public class AuctionTagsChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public string[] Tags { get; }

        public AuctionTagsChanged(Guid auctionId, string[] tags) : base(EventNames.AuctionTagsChanged)
        {
            AuctionId = auctionId;
            Tags = tags;
        }
    }
}
