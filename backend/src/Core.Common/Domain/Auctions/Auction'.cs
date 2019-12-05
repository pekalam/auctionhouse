using System;
using Core.Common.Domain.Auctions.Events;
using Core.Common.Domain.Auctions.Events.Update;
using Core.Common.Exceptions;
using ReflectionMagic;

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
            try
            {
                this.AsDynamic().ApplyEvent(@event);
            }
            catch (Exception)
            {
                throw new DomainException($"Unrecognized event: {@event.EventName}");
            }
        }

        protected override AuctionUpdateEventGroup CreateUpdateEventGroup()
        {
            return new AuctionUpdateEventGroup(Owner);
        }

        private void ApplyEvent(AuctionCreated @event)
        {
            AggregateId = @event.AuctionId;
            Create(@event.AuctionArgs, false);
        }

        private void ApplyEvent(BidCanceled @event) => CancelBid(@event.CanceledBid);
        private void ApplyEvent(AuctionRaised @event) => Raise(@event.Bid);
        private void ApplyEvent(AuctionCompleted @event) => EndAuction();
        private void ApplyEvent(AuctionImageAdded @event) => AddImage(@event.AddedImage);
        private void ApplyEvent(AuctionCanceled @event) => CancelAuction();
        private void ApplyEvent(AuctionImageRemoved @event) => RemoveImage(@event.ImgNum);
        private void ApplyEvent(AuctionImageReplaced @event) => ReplaceImage(@event.NewImage, @event.ImgNum);


        private void ApplyEvent(AuctionBuyNowPriceChanged ev) => UpdateBuyNowPrice(ev.BuyNowPrice);
        private void ApplyEvent(AuctionUpdateEventGroup group) => group.UpdateEvents.ForEach(ev => Apply(ev));
        private void ApplyEvent(AuctionEndDateChanged ev) => UpdateEndDate(ev.Date);
        private void ApplyEvent(AuctionTagsChanged ev) => UpdateTags(ev.Tags);
        private void ApplyEvent(AuctionCategoryChanged ev) => UpdateCategory(ev.Category);
        private void ApplyEvent(AuctionNameChanged ev) => UpdateName(ev.AuctionName);
        private void ApplyEvent(AuctionDescriptionChanged ev) => UpdateDescription(ev.Description);


    }
}