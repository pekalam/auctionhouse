using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public class EventConsumerDependencies
    {
        public IAppEventBuilder AppEventBuilder { get; set; } = null!;
        public Lazy<ISagaNotifications> SagaNotifications { get; set; } = null!;
        public Lazy<IImmediateNotifications> ImmediateNotifications { get; set; } = null!;
        
        public EventConsumerDependencies(IAppEventBuilder appEventBuilder, Lazy<ISagaNotifications> sagaNotifications, Lazy<IImmediateNotifications> immediateNotifications)
        {
            AppEventBuilder = appEventBuilder;
            SagaNotifications = sagaNotifications;
            ImmediateNotifications = immediateNotifications;
        }
    }

    public abstract class EventConsumer<T, TImpl> : IEventDispatcher where T : Event where TImpl : EventConsumer<T, TImpl>
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger<TImpl> _logger;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;

        protected EventConsumer(ILogger<TImpl> logger, EventConsumerDependencies dependencies)
        {
            _appEventBuilder = dependencies.AppEventBuilder;
            _logger = logger;
            _sagaNotifications = dependencies.SagaNotifications;
            _immediateNotifications = dependencies.ImmediateNotifications;
        }

        public abstract Task Consume(IAppEvent<T> appEvent);

        async Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            await Consume(_appEventBuilder
              .WithEvent(msg.Event)
              .WithCommandContext(msg.CommandContext)
              .WithReadModelNotificationsMode(msg.ReadModelNotifications)
              .Build<T>());

            if (msg.ReadModelNotifications == ReadModelNotificationsMode.Saga)
            {
                await _sagaNotifications.Value.MarkEventAsHandled(msg.CommandContext.CorrelationId, msg.Event);
            }
            if (msg.ReadModelNotifications == ReadModelNotificationsMode.Immediate)
            {
                await _immediateNotifications.Value.NotifyCompleted(msg.CommandContext.CorrelationId);
            }
            // successful event handling always results in completed request status
            //var requestStatus = RequestStatus.CreateFromCommandContext(msg.CommandContext, Status.COMPLETED);
            //if (msg.ReadModelNotifications == ReadModelNotificationsMode.Saga)
            //{
            //    await _sagaNotificationsFactory().MarkEventAsHandled(msg.CommandContext.CorrelationId, msg.Event);
            //}
            //else if (msg.ReadModelNotifications == ReadModelNotificationsMode.Immediate)
            //{
            //    _readModelNotificationsFactory().NotifyUser(requestStatus, msg.CommandContext.User);
            //}
        }
    }
}