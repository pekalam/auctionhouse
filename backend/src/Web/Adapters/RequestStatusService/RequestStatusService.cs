using System;
using System.Collections.Generic;
using System.Security.Claims;
using Core.Common;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Web.Dto.Commands;

namespace Web.Adapters.EventSignaling
{
    public class RequestStatusService : IRequestStatusSender
    {
        private readonly IHubContext<ApplicationHub> _hubContext;
        private readonly ILogger<RequestStatusService> _logger;

        public RequestStatusService(IHubContext<ApplicationHub> hubContext, ILogger<RequestStatusService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            _logger.LogDebug($"Sending completion event {appEvent.Event.EventName} to {user} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.ToString())
                .SendAsync("completed", (RequestStatusDto)new RequestStatus(appEvent.CorrelationId, Status.COMPLETED, values));
        }

        public void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent, Dictionary<string, object> values = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            _logger.LogDebug($"Sending failure event {appEvent.Event.EventName} to {user} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.ToString())
                .SendAsync("failed", (RequestStatusDto)new RequestStatus(appEvent.CorrelationId, Status.FAILED, values));
        }

        public void TrySendReqestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            try
            {
                SendRequestCompletionToUser(appEvent, user, values);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user}");
            }
        }

        public void TrySendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            try
            {
                SendRequestFailureToUser(appEvent, user, values);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user}");
            }
        }

        public void TrySendRequestCompletionToUser(string signalName, CorrelationId correlationId, Guid user, Dictionary<string, object> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user} with correlationId {correlationId.Value}");
                _hubContext.Clients.User(user.ToString())
                    .SendAsync("completed", (RequestStatusDto)new RequestStatus(correlationId, Status.COMPLETED, values));
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send event completion to user {user} {e.ToString()}");
            }
        }

        public void TrySendRequestFailureToUser(string signalName, CorrelationId correlationId, Guid user,
            Dictionary<string, object> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user} with correlationId {correlationId.Value}");
                _hubContext.Clients.User(user.ToString())
                    .SendAsync("failed", (RequestStatusDto)new RequestStatus(correlationId, Status.FAILED, values));
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send event completion to user {user} {e.ToString()}");
            }
        }

        public void TrySendNotificationToAll(string notificationName, Dictionary<string, object> values)
        {
            try
            {
                _logger.LogDebug($"Sending notification {notificationName} to all listening users");
                _hubContext.Clients.All.SendAsync(notificationName, values);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send notification {notificationName} to all listening users {e.ToString()}");
            }
        }
    }
}
