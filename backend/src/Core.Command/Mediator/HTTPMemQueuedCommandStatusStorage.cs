using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Common;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Command.Mediator
{
    internal class HTTPMemQueuedCommandStorageItem
    {
        public DateTime DateCreated { get; }
        public RequestStatus RequestStatus { get; set; }
        public ICommand Command { get; set; }

        public HTTPMemQueuedCommandStorageItem(DateTime dateCreated, RequestStatus requestStatus, ICommand command)
        {
            DateCreated = dateCreated;
            RequestStatus = requestStatus;
            Command = command;
        }
    }

    public class HTTPMemQueuedCommandStatusStorage : IHTTPQueuedCommandStatusStorage
    {
        private const int MAX_SIZE = 80_021;
        private Dictionary<string, HTTPMemQueuedCommandStorageItem> _store 
            = new Dictionary<string, HTTPMemQueuedCommandStorageItem>(MAX_SIZE);

        public void SaveStatus(RequestStatus status, ICommand command)
        {
            if (_store.Count == MAX_SIZE)
            {
                //TODO
                var minDate = _store.Values.Min(v => v.DateCreated);
                var toRemove = _store.Where(kv => kv.Value.DateCreated == minDate).Select(kv => kv.Key).FirstOrDefault();
                _store.Remove(toRemove);
            }
            var item = new HTTPMemQueuedCommandStorageItem(DateTime.UtcNow, status, command);
            _store.Add(item.RequestStatus.CorrelationId.Value, item);
        }

        public (RequestStatus, ICommand) GetCommandStatus(CorrelationId correlationId)
        {
            if (!_store.TryGetValue(correlationId.Value, out var item))
            {
                return (null, null);
            }

            if (item.RequestStatus.Status != Status.PENDING)
            {
                _store.Remove(item.RequestStatus.CorrelationId.Value);
            }

            return (item.RequestStatus, item.Command);
        }

        public void UpdateCommandStatus(RequestStatus status, ICommand command)
        {
            if (!_store.ContainsKey(status.CorrelationId.Value))
            {
                throw new KeyNotFoundException($"Cannot find request status with correlation id: {status.CorrelationId.Value}");
            }
            var item = _store[status.CorrelationId.Value];
            if (item.RequestStatus.Status == Status.COMPLETED)
            {
                return;
            }
            item.RequestStatus = status;
            item.Command = command;
        }
    }
}