using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public abstract class EventConsumer<T> : IEventDispatcher where T : Event
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger _logger;
        private readonly Func<ISagaNotifications> _sagaNotificationsFactory;

        protected EventConsumer(IAppEventBuilder appEventBuilder, ILogger logger, Func<ISagaNotifications> sagaNotificationsFactory)
        {
            _appEventBuilder = appEventBuilder;
            _logger = logger;
            _sagaNotificationsFactory = sagaNotificationsFactory;
        }

        public abstract void Consume(IAppEvent<T> appEvent);

        Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            Consume(_appEventBuilder
              .WithEvent(msg.Event)
              .WithCommandContext(msg.CommandContext)
              .WithReadModelNotificationsMode(msg.ReadModelNotifications)
              .Build<T>());
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
            return Task.CompletedTask;
        }
    }
}