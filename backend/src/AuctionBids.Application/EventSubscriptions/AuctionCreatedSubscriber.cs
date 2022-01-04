using AuctionBids.Domain.Repositories;
using Auctions.DomainEvents;
using Common.Application.Events;

namespace AuctionBids.Application.EventSubscriptions
{
    using AuctionBids.Domain;

    public class AuctionCreatedSubscriber : EventSubscriber<AuctionCreated>
    {
        private readonly IAuctionBidsRepository _allAuctionBids;
        private readonly EventBusFacade _eventBusFacade;

        public AuctionCreatedSubscriber(IAppEventBuilder appEventBuilder, IAuctionBidsRepository auctionBids, EventBusFacade eventBusFacade) : base(appEventBuilder)
        {
            _allAuctionBids = auctionBids;
            _eventBusFacade = eventBusFacade;
        }

        public override Task Handle(IAppEvent<AuctionCreated> appEvent)
        {
            var auctionBids = AuctionBids.CreateNew(new(appEvent.Event.AuctionId), new(appEvent.CommandContext.User!.Value));
            _allAuctionBids.Add(auctionBids);
            _eventBusFacade.Publish(auctionBids.PendingEvents, appEvent.CommandContext, ReadModelNotificationsMode.Saga);
            return Task.CompletedTask;
        }
    }
}
