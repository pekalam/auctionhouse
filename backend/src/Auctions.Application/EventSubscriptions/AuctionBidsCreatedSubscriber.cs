using Common.Application.Events;
using AuctionBids.DomainEvents;
using Chronicle;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain.Services;

namespace Auctions.Application.EventSubscriptions
{
    public class AuctionBidsCreatedSubscriber : EventSubscriber<Events.V1.AuctionBidsCreated>
    {
        private readonly ISagaCoordinator _sagaCoordinator;

        public AuctionBidsCreatedSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public override async Task Handle(IAppEvent<Events.V1.AuctionBidsCreated> appEvent)
        {
            var context = SagaContext
                .Create()
                .WithSagaId(appEvent.CommandContext.CorrelationId.Value)
                .WithMetadata(CreateAuctionSaga.CorrelationIdKey, appEvent.CommandContext.CorrelationId)
                .Build();

            await _sagaCoordinator.ProcessAsync(appEvent.Event, context);
        }
    }
}
