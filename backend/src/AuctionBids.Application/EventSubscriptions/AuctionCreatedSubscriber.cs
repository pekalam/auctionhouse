using AuctionBids.Domain.Repositories;
using Auctions.DomainEvents;
using Common.Application.Events;

namespace AuctionBids.Application.EventSubscriptions
{
    using AuctionBids.Domain;

    public class AuctionCreatedSubscriber : EventSubscriber<AuctionCreated>
    {
        private readonly IAuctionBidsRepository _allAuctionBids;
        private readonly EventBusHelper _eventBusHelper;

        public AuctionCreatedSubscriber(IAppEventBuilder appEventBuilder, IAuctionBidsRepository auctionBids, EventBusHelper eventBusHelper) : base(appEventBuilder)
        {
            _allAuctionBids = auctionBids;
            _eventBusHelper = eventBusHelper;
        }

        public override Task Handle(IAppEvent<AuctionCreated> appEvent)
        {
            var auctionBids = AuctionBids.CreateNew(new(appEvent.Event.AuctionId), new(appEvent.CommandContext.User!.Value));
            _allAuctionBids.Add(auctionBids);
            _eventBusHelper.Publish(auctionBids.PendingEvents, appEvent.CommandContext, ReadModelNotificationsMode.Saga);
            return Task.CompletedTask;
        }
    }
}
