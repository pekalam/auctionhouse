using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, HTTPMemQueuedCommandStorageItem> _store 
            = new Dictionary<string, HTTPMemQueuedCommandStorageItem>();

        internal DateTime Now { get; set; } = DateTime.UtcNow;

        internal void SaveStatus(HTTPMemQueuedCommandStorageItem item)
        {
            _store.Add(item.RequestStatus.CorrelationId.Value, item);
        }

        public void SaveStatus(RequestStatus status, ICommand command)
        {
            var item = new HTTPMemQueuedCommandStorageItem(DateTime.UtcNow, status, command);
            SaveStatus(item);
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
            item.RequestStatus = status;
            item.Command = command;
        }

        public void Cleanup()
        {
            var toRemove = _store.Values
                .Where(v => Now.Subtract((DateTime) v.DateCreated).Seconds > 30)
                .Select(v => v.RequestStatus.CorrelationId.Value)
                .ToArray();
            foreach (var key in toRemove)
            {
                _store.Remove(key);
            }
        }
    }
}