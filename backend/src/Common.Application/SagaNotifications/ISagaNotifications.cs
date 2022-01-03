using Common.Application.Events;
using Core.Common.Domain;

namespace Common.Application.SagaNotifications
{
    public interface ISagaNotifications
    {
        Task RegisterNewSaga(CorrelationId correlationId);
        Task MarkSagaAsCompleted(CorrelationId correlationId);
        Task MarkSagaAsFailed(CorrelationId correlationId);
        Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event;
        Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event;
        Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> @event) where T : Event;
    }

    public class InMemorySagaNotifications : ISagaNotifications
    {
        private readonly InMemorySagaEventsConfirmationStore _sagaEventsConfirmationStore = new();

        public Task AddUnhandledEvent<T>(CorrelationId correlationId, T @event) where T : Event
        {
            return Task.CompletedTask;

        }

        public Task AddUnhandledEvents<T>(CorrelationId correlationId, IEnumerable<T> @event) where T : Event
        {
            return Task.CompletedTask;

        }

        public Task MarkEventAsHandled<T>(CorrelationId correlationId, T @event) where T : Event
        {
            return Task.CompletedTask;

        }

        public Task MarkSagaAsCompleted(CorrelationId correlationId)
        {
            return Task.CompletedTask;

        }

        public Task MarkSagaAsFailed(CorrelationId correlationId)
        {
            return Task.CompletedTask;
        }

        public Task RegisterNewSaga(CorrelationId correlationId)
        {
            return Task.CompletedTask;
        }
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