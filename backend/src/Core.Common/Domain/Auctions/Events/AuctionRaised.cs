using Core.Common.Domain.Bids;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionRaised : Event
    {
        public AuctionRaised(Bid bid) : base(EventNames.AuctionRaisedEventName)
        {
            Bid = bid;
        }

        public Bid Bid { get; }
    }
}
