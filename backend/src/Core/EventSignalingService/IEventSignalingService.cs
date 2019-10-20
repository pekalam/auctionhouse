using System.Collections.Generic;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;

namespace Core.Common.EventSignalingService
{
    public interface IEventSignalingService
    {
        void SendEventCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void SendEventCompletionToAll<T>(IAppEvent<T> appEvent) where T : Event;
        void SendEventFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void TrySendEventCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void TrySendEventFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;

        void TrySendCompletionToUser(string signalName, CorrelationId correlationId, UserIdentity user,
            Dictionary<string, string> values = null);


    }
}