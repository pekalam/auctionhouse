using Auctions.Application.Sagas;
using Chronicle;
using Common.Application.Events;
using static UserPayments.DomainEvents.Events.V1;

namespace Auctions.Application.EventSubscriptions
{
    public class PaymentCreationErrorSubscriber : EventSubscriber<PaymentCreationError>
    {
        private readonly ISagaCoordinator _sagaCoordinator;

        public PaymentCreationErrorSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public override Task Handle(IAppEvent<PaymentCreationError> appEvent)
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
