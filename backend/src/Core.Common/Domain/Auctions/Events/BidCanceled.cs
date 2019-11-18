using Core.Common.Domain.Bids;

namespace Core.Common.Domain.Auctions.Events
{
    class BidCanceled : Event
    {
        public Bid CanceledBid { get; }

        public BidCanceled(Bid canceledBid) : base(EventNames.BidCanceled)
        {
            CanceledBid = canceledBid;
        }
    }
}
