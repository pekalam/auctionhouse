using Core.Common;
using Core.Common.Command;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly Dictionary<string, HTTPMemQueuedCommandStorageItem> _store
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
            _store.Add(status.CommandId.Id, item);
        }

        public (RequestStatus, ICommand) GetCommandStatus(CommandId commandId)
        {
            if (!_store.TryGetValue(commandId.Id, out var item))
            {
                return (null, null);
            }

            if (item.RequestStatus.Status != Status.PENDING)
            {
                _store.Remove(item.RequestStatus.CommandId.Id);
            }

            return (item.RequestStatus, item.Command);
        }

        public void UpdateCommandStatus(RequestStatus status, CommandId commandId)
        {
            if (!_store.ContainsKey(commandId.Id))
            {
                throw new KeyNotFoundException($"Cannot find request status with correlation id: {commandId.Id}");
            }
            var item = _store[commandId.Id];
            if (item.RequestStatus.Status == Status.COMPLETED)
            {
                return;
            }
            item.RequestStatus = status;
        }
    }
}