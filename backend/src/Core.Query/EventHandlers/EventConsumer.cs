using System;
using Core.Common.Domain;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public abstract class EventConsumer<T> : IEventConsumer where T : Event
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger _logger;

        protected EventConsumer(IAppEventBuilder appEventBuilder, ILogger logger)
        {
            _appEventBuilder = appEventBuilder;
            _logger = logger;
        }

        Type IEventConsumer.MessageType => typeof(T);

        void IEventConsumer.Dispatch(object message)
        {
            IAppEvent<Event> appEventBase = (IAppEvent<Event>) message;

            this.Consume(_appEventBuilder
                .WithCorrelationId(appEventBase.CorrelationId)
                .WithEvent(appEventBase.Event)
                .WithCommand(appEventBase.Command)
                .Build<T>());
        }

        public abstract void Consume(IAppEvent<T> appEvent);
    }
}