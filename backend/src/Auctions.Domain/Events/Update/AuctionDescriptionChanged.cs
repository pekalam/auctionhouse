using Core.Common.Domain;

namespace Auctions.Domain.Events.Update
{
    public class AuctionDescriptionChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public string Description { get; }

        public AuctionDescriptionChanged(Guid auctionId, string description) : base(EventNames.AuctionDescriptionChanged)
        {
            AuctionId = auctionId;
            Description = description;
        }
    }
}
