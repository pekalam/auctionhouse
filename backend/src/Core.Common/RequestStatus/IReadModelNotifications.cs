using System;
using System.Collections.Generic;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Core.Common.RequestStatusSender
{
    public interface IReadModelNotifications
    {
        void NotifyUser<T>(RequestStatus requestStatus, IAppEvent<T> appEvent, Guid user, Dictionary<string, object> values = null) where T : Event;
        void NotifyUser(RequestStatus requestStatus, Guid user, Dictionary<string, object> values = null);
        void NotifyAll<T>(RequestStatus requestStatus, IAppEvent<T> appEvent, Dictionary<string, object> values = null) where T : Event;
    }
}