using System;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Domain;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public abstract class EventConsumer<T> : IEventConsumer where T : Event
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger _logger;
        private readonly Func<ISagaNotifications> _sagaNotificationsFactory;
        private readonly Func<IReadModelNotifications> _readModelNotificationsFactory;

        protected EventConsumer(IAppEventBuilder appEventBuilder, ILogger logger, Func<ISagaNotifications> sagaNotificationsFactory, Func<IReadModelNotifications> readModelNotificationsFactory)
        {
            _appEventBuilder = appEventBuilder;
            _logger = logger;
            _sagaNotificationsFactory = sagaNotificationsFactory;
            _readModelNotificationsFactory = readModelNotificationsFactory;
        }

        Type IEventConsumer.MessageType => typeof(T);

        async void IEventConsumer.Dispatch(object message)
        {
            IAppEvent<Event> appEventBase = (IAppEvent<Event>)message;

            Consume(_appEventBuilder
                .WithEvent(appEventBase.Event)
                .WithCommandContext(appEventBase.CommandContext)
                .WithReadModelNotificationsMode(appEventBase.ReadModelNotifications)
                .Build<T>());
            // successful event handling always results in completed request status
            var requestStatus = RequestStatus.CreateFromCommandContext(appEventBase.CommandContext, Status.COMPLETED); 
            if (appEventBase.ReadModelNotifications == ReadModelNotificationsMode.Saga)
            {
                await _sagaNotificationsFactory().MarkEventAsHandled(appEventBase.CommandContext.CorrelationId, appEventBase.Event);
            }
            else if (appEventBase.ReadModelNotifications == ReadModelNotificationsMode.Immediate)
            {
                _readModelNotificationsFactory().NotifyUser(requestStatus, appEventBase.CommandContext.User);
            }
        }

        public abstract void Consume(IAppEvent<T> appEvent);
    }
}