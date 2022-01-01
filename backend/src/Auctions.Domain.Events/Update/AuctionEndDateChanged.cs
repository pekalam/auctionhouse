using Core.Common.Domain;

namespace Auctions.DomainEvents.Update
{
    public class AuctionEndDateChanged : UpdateEvent
    {
        public Guid AuctionId { get; }
        public DateTime Date { get; }

        public AuctionEndDateChanged(Guid auctionId, DateTime date) : base(EventNames.AuctionEndDateChanged)
        {
            AuctionId = auctionId;
            Date = date;
        }
    }
}
