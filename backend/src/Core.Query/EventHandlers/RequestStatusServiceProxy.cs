using System;
using System.Collections.Generic;
using System.Text;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Core.Query.Exceptions;

namespace Core.Query.EventHandlers
{
    public class RequestStatusServiceProxy : IRequestStatusSender
    {
        private readonly IRequestStatusSender _wsRequestStatusService;
        private readonly IHTTPQueuedCommandStatusStorage _httpQueuedCommandStatusStorage;

        public RequestStatusServiceProxy(IRequestStatusSender wsRequestStatusService, IHTTPQueuedCommandStatusStorage httpQueuedCommandStatusStorage)
        {
            _wsRequestStatusService = wsRequestStatusService;
            _httpQueuedCommandStatusStorage = httpQueuedCommandStatusStorage;
        }

        private void SendHttpCompleted<T>(IAppEvent<T> appEvent, Dictionary<string, object> values)
            where T : Event
        {
            var requestStatus =
                RequestStatus.CreateFromCommandContext(appEvent.CommandBase.CommandContext, Status.COMPLETED, values);
            _httpQueuedCommandStatusStorage.UpdateCommandStatus(requestStatus, appEvent.CommandBase);
        }

        private void SendHttpFailed<T>(IAppEvent<T> appEvent, Dictionary<string, object> values) where T : Event
        {
            var requestStatus =
                RequestStatus.CreateFromCommandContext(appEvent.CommandBase.CommandContext, Status.FAILED, values);
            _httpQueuedCommandStatusStorage.UpdateCommandStatus(requestStatus, appEvent.CommandBase);
        }

        private void TrySendHttpCompleted<T>(IAppEvent<T> appEvent, Dictionary<string, object> values)
            where T : Event
        {
            try
            {
                SendHttpCompleted(appEvent, values);
            }
            catch (Exception)
            {
            }
        }

        private void TrySendHttpFailed<T>(IAppEvent<T> appEvent, Dictionary<string, object> values)
            where T : Event
        {
            try
            {
                SendHttpFailed(appEvent, values);
            }
            catch (Exception)
            {
            }
        }

        public void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            if (appEvent.CommandBase.HttpQueued)
            {
                SendHttpCompleted(appEvent, values);
            }
            else
            {
                _wsRequestStatusService.SendRequestCompletionToUser(appEvent, user, values);
            }
        }

        public void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent, Dictionary<string, object> values = null) where T : Event
        {
            if (appEvent.CommandBase.HttpQueued)
            {
                throw new QueryException("Cannot send request status to all of http queued command");
            }

            _wsRequestStatusService.SendRequestCompletionToAll(appEvent, values);
        }

        public void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            if (appEvent.CommandBase.HttpQueued)
            {
                SendHttpFailed(appEvent, values);
            }
            else
            {
                _wsRequestStatusService.SendRequestFailureToUser(appEvent, user, values);
            }
        }

        public void TrySendReqestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user,
            Dictionary<string, object> values = null) where T : Event
        {
            if (appEvent.CommandBase.HttpQueued)
            {
                TrySendHttpCompleted(appEvent, values);
            }
            else
            {
                _wsRequestStatusService.TrySendReqestCompletionToUser(appEvent, user, values);
            }
        }

        public void TrySendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user,
            Dictionary<string, object> values = null) where T : Event
        {
            if (appEvent.CommandBase.HttpQueued)
            {
                TrySendHttpFailed(appEvent, values);
            }
            else
            {
                _wsRequestStatusService.TrySendRequestFailureToUser(appEvent, user, values);
            }
        }

        public void TrySendRequestCompletionToUser(string signalName, CorrelationId correlationId, Guid user,
            Dictionary<string, object> values = null)
        {
            _wsRequestStatusService.TrySendRequestCompletionToUser(signalName, correlationId, user, values);
        }

        public void TrySendRequestFailureToUser(string signalName, CorrelationId correlationId, Guid user,
            Dictionary<string, object> values = null)
        {
            _wsRequestStatusService.TrySendRequestFailureToUser(signalName, correlationId, user, values);
        }

        public void TrySendNotificationToAll(string notificationName, Dictionary<string, object> values)
        {
            _wsRequestStatusService.TrySendNotificationToAll(notificationName, values);
        }
    }
}