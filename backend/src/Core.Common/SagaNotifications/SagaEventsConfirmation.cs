using System.Collections.Generic;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Common
{
    public class SagaEventsConfirmation
    {
        public CorrelationId CorrelationId { get; }
        public CommandId CommandId { get; }
        /// <summary>
        /// Events required to be processed by query side consumers when they have <see cref="IAppEvent{T}.ReadModelNotifications"/> = <see cref="ReadModelNotificationsMode.Saga"/>.
        /// When this collection is changed then it must be persisted before these events are published.
        /// </summary>
        public HashSet<string> UnprocessedEvents { get; }
        public HashSet<string> ProcessedEvents { get; }
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }
        public bool AllConfirmed => UnprocessedEvents.Count == 0;

        public SagaEventsConfirmation(CorrelationId correlationId, CommandId commandId, HashSet<string> unprocessedEvents, HashSet<string> processedEvents, bool isCompleted)
        {
            CorrelationId = correlationId;
            CommandId = commandId;
            UnprocessedEvents = unprocessedEvents;
            ProcessedEvents = processedEvents;
            IsCompleted = isCompleted;
        }

        public static SagaEventsConfirmation CreateNew(CommandId commandId, CorrelationId correlationId) => new SagaEventsConfirmation(correlationId, commandId, new(), new(), false);

        public void SetCompleted() => IsCompleted = true;

        public bool AddUnprocessedEvent(string @event)
        {
            var processedContains = ProcessedEvents.Contains(@event);
            if (processedContains) return false;
            var unprocessedContains = UnprocessedEvents.Contains(@event);
            if (unprocessedContains) return false;
            UnprocessedEvents.Add(@event);
            return true;
        }

        public bool MarkEventAsProcessed(string @event)
        {
            var processedContains = ProcessedEvents.Contains(@event);
            if (processedContains) return false;
            var unprocessedContains = UnprocessedEvents.Contains(@event);
            if (!unprocessedContains) return false;
            UnprocessedEvents.Remove(@event);
            ProcessedEvents.Add(@event);
            return true;
        }
    }
}