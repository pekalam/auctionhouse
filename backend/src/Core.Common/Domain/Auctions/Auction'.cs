using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Auctions.Events.Update;
using Core.Common.Exceptions;

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
            //TODO
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
            else if(@event is AuctionBuyNowPriceChanged)
                Apply(@event as AuctionBuyNowPriceChanged);
            else if (@event is AuctionEndDateChanged)
                Apply(@event as AuctionEndDateChanged);
            else if (@event is AuctionTagsChanged)
                Apply(@event as AuctionTagsChanged);
            else if(@event is AuctionCategoryChanged)
                Apply(@event as AuctionCategoryChanged);
            else if (@event is AuctionNameChanged)
                Apply(@event as AuctionNameChanged);
            else if (@event is UpdateEventGroup)
                Apply(@event as UpdateEventGroup);
            else
            {
                throw new DomainException($"Unrecognized event: {@event.EventName}");
            }
        }

        protected override AuctionUpdateEventGroup CreateUpdateEventGroup()
        {
            return new AuctionUpdateEventGroup(Owner);
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


        private void Apply(AuctionBuyNowPriceChanged ev) => SetBuyNowPrice(ev.BuyNowPrice);
        private void Apply(UpdateEventGroup group) => group.UpdateEvents.ForEach(ev => Apply(ev));
        private void Apply(AuctionEndDateChanged ev) => SetEndDate(ev.Date);
        private void Apply(AuctionTagsChanged ev) => SetTags(ev.Tags);
        private void Apply(AuctionCategoryChanged ev) => SetCategory(ev.Category);
        private void Apply(AuctionNameChanged ev) => SetName(ev.AuctionName);


    }
}