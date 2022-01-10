using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public abstract class EventConsumer<T, TImpl> : IEventDispatcher where T : Event where TImpl : EventConsumer<T, TImpl>
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger<TImpl> _logger;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;

        protected EventConsumer(IAppEventBuilder appEventBuilder, ILogger<TImpl> logger, Lazy<ISagaNotifications> sagaNotificationsFactory, Lazy<IImmediateNotifications> immediateNotifications)
        {
            _appEventBuilder = appEventBuilder;
            _logger = logger;
            _sagaNotifications = sagaNotificationsFactory;
            _immediateNotifications = immediateNotifications;
        }

        //TODO TASK return type
        public abstract void Consume(IAppEvent<T> appEvent);

        async Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            Consume(_appEventBuilder
              .WithEvent(msg.Event)
              .WithCommandContext(msg.CommandContext)
              .WithReadModelNotificationsMode(msg.ReadModelNotifications)
              .Build<T>());

            if(msg.ReadModelNotifications == ReadModelNotificationsMode.Saga)
            {
                await _sagaNotifications.Value.MarkEventAsHandled(msg.CommandContext.CorrelationId, msg.Event);
            }
            if(msg.ReadModelNotifications == ReadModelNotificationsMode.Immediate)
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