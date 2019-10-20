using System;
using System.Collections.Generic;
using System.Security.Claims;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Web.Adapters.EventSignaling
{
    public class EventSignalingService : IEventSignalingService
    {
        private readonly IHubContext<ApplicationHub> _hubContext;
        private readonly ILogger<EventSignalingService> _logger;

        public EventSignalingService(IHubContext<ApplicationHub> hubContext, ILogger<EventSignalingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void SendEventCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            _logger.LogDebug($"Sending completion event {appEvent.Event.EventName} to {user.UserName} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.UserId.ToString())
                .SendAsync("completed", appEvent.Event.EventName, appEvent.CorrelationId.Value);
        }

        public void SendEventCompletionToAll<T>(IAppEvent<T> appEvent) where T : Event
        {
            throw new NotImplementedException();
        }

        public void SendEventFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            _logger.LogDebug($"Sending failure event {appEvent.Event.EventName} to {user.UserName} with correlationId {appEvent.CorrelationId.Value}");
            _hubContext.Clients.User(user.UserId.ToString())
                .SendAsync("failure", appEvent.Event.EventName, appEvent.CorrelationId.Value);
        }

        public void TrySendEventCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            try
            {
                SendEventCompletionToUser(appEvent, user);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName}");
            }
        }

        public void TrySendEventFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event
        {
            try
            {
                SendEventFailureToUser(appEvent, user);
            }
            catch (Exception)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName}");
            }
        }

        public void TrySendCompletionToUser(string signalName, CorrelationId correlationId, UserIdentity user, Dictionary<string, string> values = null)
        {
            try
            {
                _logger.LogDebug($"Sending completed signal {signalName} to {user.UserName} with correlationId {correlationId.Value}");
                if (values == null)
                {
                    _hubContext.Clients.User(user.UserId.ToString())
                        .SendAsync("completed", signalName, correlationId.Value);
                }
                else
                {
                    _hubContext.Clients.User(user.UserId.ToString())
                        .SendAsync("completed", signalName, correlationId.Value, values);
                }
                
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Cannot send event completion to user {user.UserName} {e.ToString()}");
            }
        }
    }
}
