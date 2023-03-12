using Chronicle;
using Common.Application.Events;
using UserPayments.Domain;
using Users.Application.Sagas;

namespace Users.Application.EventSubscriptions
{
    public class UserPaymentsCreatedEventSubscriber : EventSubscriber<UserPaymentsCreated>
    {
        private readonly ISagaCoordinator _sagaCoordinator;

        public UserPaymentsCreatedEventSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public override async Task Handle(IAppEvent<UserPaymentsCreated> appEvent)
        {
            var context = SagaContext
                .Create()
                .WithSagaId(appEvent.CommandContext.CorrelationId.Value)
                .WithMetadata(SignUpSaga.CmdContextParamName, appEvent.CommandContext)
                .Build();

            await _sagaCoordinator.ProcessAsync(appEvent.Event, context);
        }
    }
}
