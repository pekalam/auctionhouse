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
        private readonly CreateAuctionService _createAuctionService;

        public AuctionBidsCreatedSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator, CreateAuctionService createAuctionService) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
            _createAuctionService = createAuctionService;
        }

        public override async Task Handle(IAppEvent<Events.V1.AuctionBidsCreated> appEvent)
        {
            var context = SagaContext
                .Create()
                .WithSagaId(appEvent.CommandContext.CorrelationId.Value)
                .WithMetadata(CreateAuctionSaga.ServiceDataKey, _createAuctionService.ServiceData)
                .Build();
            await _sagaCoordinator.ProcessAsync(appEvent.Event, context);

        }
    }
}
