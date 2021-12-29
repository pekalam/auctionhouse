using System;
using System.Collections.Generic;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.EventBus;

namespace Core.Common.RequestStatusSender
{
    public interface IRequestStatusSender
    {
        void SendRequestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event;
        void SendRequestCompletionToAll<T>(IAppEvent<T> appEvent, Dictionary<string, object> values = null) where T : Event;
        void SendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event;

        void TrySendReqestCompletionToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event;
        void TrySendRequestFailureToUser<T>(IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event;
        void TrySendRequestCompletionToUser(string signalName, CommandId commandId, Guid user,
            Dictionary<string, object> values = null);
        void TrySendRequestFailureToUser(string signalName, CommandId commandId, Guid user,
            Dictionary<string, object> values = null);


        void TrySendNotificationToAll(string notificationName, Dictionary<string, object> values);
    }
}