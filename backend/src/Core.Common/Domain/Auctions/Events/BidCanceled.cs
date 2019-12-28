using Core.Common.Domain.Bids;

namespace Core.Common.Domain.Auctions.Events
{
    public class BidCanceled : Event
    {
        public Bid CanceledBid { get; }
        public Bid NewWinner { get; }

        public BidCanceled(Bid canceledBid, Bid newWinner) : base(EventNames.BidCanceled)
        {
            CanceledBid = canceledBid;
            NewWinner = newWinner;
        }
    }
}
