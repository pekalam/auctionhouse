using Auctions.Application.Sagas;
using Chronicle;
using Common.Application.Events;
using UserPayments.DomainEvents;

namespace Auctions.Application.EventSubscriptions
{
    public class BuyNowPaymentConfirmedSubscriber : EventSubscriber<Events.V1.BuyNowPaymentConfirmed>
    {
        private readonly ISagaCoordinator _sagaCoordinator;

        public BuyNowPaymentConfirmedSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public override Task Handle(IAppEvent<Events.V1.BuyNowPaymentConfirmed> appEvent)
        {
            var context = SagaContext
                .Create()
                .WithSagaId(appEvent.CommandContext.CorrelationId.Value)
                .WithMetadata(BuyNowSaga.CmdContextParamName, appEvent.CommandContext)
                .Build();

            return _sagaCoordinator.ProcessAsync(appEvent.Event, context);
        }
    }
}
