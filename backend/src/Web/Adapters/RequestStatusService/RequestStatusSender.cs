using System;
using System.Collections.Generic;
using System.Security.Claims;
using Core.Common;
using Core.Common.Command;
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
    public class RequestStatusSender : IRequestStatusSender
    {
        private readonly IHubContext<ApplicationHub> _hubContext;
        private readonly ILogger<RequestStatusSender> _logger;

        public RequestStatusSender(IHubContext<ApplicationHub> hubContext, ILogger<RequestStatusSender> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            _logger.LogDebug($"Sending completion event {appEvent.Event.EventName} to {user} with commandId {appEvent.CommandContext.CorrelationId.Value}");
            _hubContext.Clients.User(user.ToString())
                .SendAsync("completed", (RequestStatusDto)new RequestStatus(appEvent.CommandContext.CommandId, Status.COMPLETED, values));
        }

        public void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent, Dictionary<string, object> values = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event
        {
            _logger.LogDebug($"Sending failure event {appEvent.Event.EventName} to {user} with commandId {appEvent.CommandContext.CorrelationId.Value}");
            _hubContext.Clients.User(user.ToString())
                .SendAsync("failed", (RequestStatusDto)new RequestStatus(appEvent.CommandContext.CommandId, Status.FAILED, values));
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

        public void TrySendRequestCompletionToUser(string signalName, CommandId commandId, Guid user, Dictionary<string, object> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user} with commandId {commandId.Id}");
                _hubContext.Clients.User(user.ToString())
                    .SendAsync("completed", (RequestStatusDto)new RequestStatus(commandId, Status.COMPLETED, values));
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send event completion to user {user} {e.ToString()}");
            }
        }

        public void TrySendRequestFailureToUser(string signalName, CommandId commandId, Guid user,
            Dictionary<string, object> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user} with commandId {commandId.Id}");
                _hubContext.Clients.User(user.ToString())
                    .SendAsync("failed", (RequestStatusDto)new RequestStatus(commandId, Status.FAILED, values));
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
