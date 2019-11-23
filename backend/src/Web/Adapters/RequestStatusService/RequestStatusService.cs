using System;
using System.Collections.Generic;
using System.Security.Claims;
using Core.Common;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Web.Dto.Commands;

namespace Web.Adapters.EventSignaling
{
    public class RequestStatusService : IRequestStatusService
    {
        private readonly IHubContext<ApplicationHub> _hubContext;
        private readonly ILogger<RequestStatusService> _logger;

        public RequestStatusService(IHubContext<ApplicationHub> hubContext, ILogger<RequestStatusService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            _logger.LogDebug($"Sending completion event {appEvent.Event.EventName} to {user.UserName} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.UserId.ToString())
                .SendAsync("completed", (RequestStatusDto)new RequestStatus(appEvent.CorrelationId, Status.COMPLETED));
        }

        public void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent) where T : Event
        {
            throw new NotImplementedException();
        }

        public void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            _logger.LogDebug($"Sending failure event {appEvent.Event.EventName} to {user.UserName} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.UserId.ToString())
                .SendAsync("failed", (RequestStatusDto)new RequestStatus(appEvent.CorrelationId, Status.FAILED));
        }

        public void TrySendReqestCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            try
            {
                SendRequestCompletionToUser(appEvent, user);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName}");
            }
        }

        public void TrySendRequestFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            try
            {
                SendRequestFailureToUser(appEvent, user);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName}");
            }
        }

        public void TrySendRequestCompletionToUser(string signalName, CorrelationId correlationId, UserIdentity user, Dictionary<string, object> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user.UserName} with correlationId {correlationId.Value}");
                _hubContext.Clients.User(user.UserId.ToString())
                    .SendAsync("completed", (RequestStatusDto)new RequestStatus(correlationId, Status.COMPLETED, values));
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName} {e.ToString()}");
            }
        }
    }
}
