using Core.Common.Domain;

namespace Auctions.Domain.Events.Update
{
    public class AuctionEndDateChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public AuctionDate Date { get; }

        public AuctionEndDateChanged(Guid auctionId, AuctionDate date) : base(EventNames.AuctionEndDateChanged)
        {
            AuctionId = auctionId;
            Date = date;
        }
    }
}
