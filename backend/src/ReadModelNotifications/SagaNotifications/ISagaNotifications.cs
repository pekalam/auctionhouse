using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;

namespace ReadModelNotifications.SagaNotifications
{
    /// <summary>
    /// Responsible for sending notification / saving outcome of saga. 
    /// Listeners use this interface to mark events that require notifications mode=<see cref="ReadModelNotificationsMode.Saga"/>
    /// </summary>
    public interface ISagaNotifications
    {
        Task RegisterNewSaga(CorrelationId correlationId, CommandId commandId);
        Task MarkSagaAsCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null);
        Task MarkSagaAsFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null);
        Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event;
        Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event;
        Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> @event) where T : Event;
    }

    internal class SagaEventsConfirmationStoreItem
    {
        public DateTime DateCreated { get; }
        public SagaEventsConfirmation SagaEventsConfirmation { get; set; }

        public SagaEventsConfirmationStoreItem(DateTime dateCreated, SagaEventsConfirmation sagaEventsConfirmation)
        {
            DateCreated = dateCreated;
            SagaEventsConfirmation = sagaEventsConfirmation;
        }
    }

    public class InMemorySagaEventsConfirmationStore
    {
        private const int MAX_SIZE = 80_021;
        private readonly Dictionary<string, SagaEventsConfirmationStoreItem> _store
            = new Dictionary<string, SagaEventsConfirmationStoreItem>(MAX_SIZE);

        public Task<SagaEventsConfirmation> GetByCorrelationId(CorrelationId correlationId)
        {
            if (_store.TryGetValue(correlationId.Value, out var item))
            {
                return Task.FromResult(item.SagaEventsConfirmation);
            }
            return Task.FromResult((SagaEventsConfirmation)null);
        }

        private void TryRemoveLast()
        {
            if (_store.Count == MAX_SIZE)
            {
                //TODO
                var minDate = _store.Values.Min(v => v.DateCreated);
                var toRemove = _store.Where(kv => kv.Value.DateCreated == minDate).Select(kv => kv.Key).FirstOrDefault();
                _store.Remove(toRemove);
            }
        }

        public Task Add(CorrelationId correlationId, SagaEventsConfirmation sagaEventsConfirmation)
        {
            TryRemoveLast();
            _store[correlationId.Value] = new SagaEventsConfirmationStoreItem(DateTime.Now, sagaEventsConfirmation);
            return Task.CompletedTask;
        }

        public Task Update(CorrelationId correlationId, SagaEventsConfirmation sagaEventsConfirmation)
        {
            TryRemoveLast();
            _store[correlationId.Value] = new SagaEventsConfirmationStoreItem(DateTime.Now, sagaEventsConfirmation);
            return Task.CompletedTask;
        }
    }
}