using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Core.Query.EventHandlers
{
    public class EventConsumerDependencies
    {
        public IAppEventBuilder AppEventBuilder { get; set; } = null!;
        public IEventConsumerCallbacks EventConsumerCallbacks { get; }

        public EventConsumerDependencies(IAppEventBuilder appEventBuilder, IEventConsumerCallbacks eventConsumerCallbacks)
        {
            AppEventBuilder = appEventBuilder;
            EventConsumerCallbacks = eventConsumerCallbacks;
        }
    }

    public abstract class EventConsumer<T, TImpl> : IEventDispatcher where T : Event where TImpl : EventConsumer<T, TImpl>
    {
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly ILogger<TImpl> _logger;
        private readonly IEventConsumerCallbacks _eventConsumerCallbacks;

        protected EventConsumer(ILogger<TImpl> logger, EventConsumerDependencies dependencies)
        {
            _appEventBuilder = dependencies.AppEventBuilder;
            _eventConsumerCallbacks = dependencies.EventConsumerCallbacks;
            _logger = logger;
        }

        public abstract Task Consume(IAppEvent<T> appEvent);

        async Task IEventDispatcher.Dispatch(IAppEvent<Event> msg)
        {
            using var activity = Tracing.StartTracing(GetType().Name + "_" + msg.Event.EventName, msg.CommandContext.CorrelationId);

            await ConsumeEvent(msg);
            await _eventConsumerCallbacks.OnEventProcessed(msg, _logger);

            activity.TraceOkStatus();
        }

        private async Task ConsumeEvent(IAppEvent<Event> msg)
        {
            try
            {
                await Consume(_appEventBuilder
                  .WithEvent(msg.Event)
                  .WithCommandContext(msg.CommandContext)
                  .WithReadModelNotificationsMode(msg.ReadModelNotifications)
                  .WithRedeliveryCount(msg.RedeliveryCount)
                  .Build<T>());
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus(e.Message);
                _logger.LogWarning(e, "Event consumer thrown an exception while consuming an event");
                throw;
            }
        }
    }
}