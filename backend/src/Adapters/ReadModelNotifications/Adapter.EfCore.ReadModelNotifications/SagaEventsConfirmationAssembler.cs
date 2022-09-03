using ReadModelNotifications.SagaNotifications;

namespace Adapter.EfCore.ReadModelNotifications
{
    using Common.Application.Events;
    using Common.Application.Commands;

    internal static class SagaEventsConfirmationAssembler
    {
        public static SagaEventsConfirmation FromDbEntity(DbSagaEventsConfirmation dbEntity, DbSagaEventToConfirm[]? eventsToConfirm = null)
        {
            var unprocessedEvents = eventsToConfirm?.Where(e => !e.Processed).Select(e => e.EventName) ?? Enumerable.Empty<string>();
            var processedEvents = eventsToConfirm?.Where(e => e.Processed).Select(e => e.EventName) ?? Enumerable.Empty<string>();
            return new SagaEventsConfirmation(new CorrelationId(dbEntity.CorrelationId),
                new CommandId(dbEntity.CommandId),
                new HashSet<string>(unprocessedEvents),
                new HashSet<string>(processedEvents), dbEntity.Completed,
                dbEntity.Failed);
        }

        public static DbSagaEventsConfirmation ToDbEntity(long id, SagaEventsConfirmation confirmations)
        {
            return new DbSagaEventsConfirmation
            {
                Id = id,
                CorrelationId = confirmations.CorrelationId.Value,
                CommandId = confirmations.CommandId.Id,
                Completed = confirmations.IsCompleted,
                Failed = confirmations.IsFailed,
            };
        }
    }
}