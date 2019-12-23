using System.Collections.Generic;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;

namespace Core.Common.RequestStatusService
{
    public interface IRequestStatusService
    {
        void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent) where T : Event;
        void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;

        void TrySendReqestCompletionToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void TrySendRequestFailureToUser<T>(IAppEvent<T> appEvent, UserIdentity user) where T : Event;
        void TrySendRequestCompletionToUser(string signalName, CorrelationId correlationId, UserIdentity user,
            Dictionary<string, object> values = null);
        void TrySendRequestFailureToUser(string signalName, CorrelationId correlationId, UserIdentity user,
            Dictionary<string, object> values = null);


        void TrySendNotificationToAll(string notificationName, Dictionary<string, object> values);
    }
}