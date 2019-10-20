using System;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public abstract class EventConsumer<T> : IEventConsumer where T : Event
    {
        private readonly IAppEventBuilder _appEventBuilder;

        protected EventConsumer(IAppEventBuilder appEventBuilder)
        {
            _appEventBuilder = appEventBuilder;
        }

        Type IEventConsumer.MessageType => typeof(T);

        void IEventConsumer.Dispatch(object message)
        {
            IAppEvent<Event> appEventBase = (IAppEvent<Event>) message;

            this.Consume(_appEventBuilder.WithCorrelationId(appEventBase.CorrelationId).WithEvent(appEventBase.Event)
                .Build<T>());
        }

        public abstract void Consume(IAppEvent<T> appEvent);
    }
}