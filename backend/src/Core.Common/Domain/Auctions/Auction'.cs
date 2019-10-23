using Core.Common.Domain.Auctions.Events;

namespace Core.Common.Domain.Auctions
{
    public partial class Auction
    {
        public override Event GetRemovedEvent()
        {
            return new AuctionRemoved(AggregateId);
        }

        protected override void Apply(Event @event)
        {
            if (@event is AuctionCreated)
                Apply(@event as AuctionCreated);
            else if (@event is BidCanceled)
                Apply(@event as BidCanceled);
            else if (@event is AuctionRaised)
                Apply(@event as AuctionRaised);
            else if (@event is AuctionCompleted)
                Apply(@event as AuctionCompleted);
            else if (@event is AuctionImageAdded)
                Apply(@event as AuctionImageAdded);
            else if (@event is AuctionCanceled)
                Apply(@event as AuctionCanceled);
            else
            {
                throw new DomainException($"Unrecognized event: {@event.EventName}");
            }
        }

        private void Apply(AuctionCreated @event)
        {
            AggregateId = @event.AuctionId;
            Create(@event.AuctionArgs, false);
        }

        private void Apply(BidCanceled @event) => CancelBid(@event.CanceledBid);
        private void Apply(AuctionRaised @event) => Raise(@event.Bid);
        private void Apply(AuctionCompleted @event) => EndAuction();
        private void Apply(AuctionImageAdded @event) => AddImage(@event.AddedImage);
        private void Apply(AuctionCanceled @event) => CancelAuction();
    }
}