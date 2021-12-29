using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Common.Command;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Common.EventBus
{
    public interface IEventBus
    {
        /// <summary>
        /// Sends event to queue named like <see cref="IAppEvent<T>"/>
        /// </summary>
        void Publish<T>(IAppEvent<T> @event) where T : Event;
        /// <summary>
        /// Sends event to queue named like each <see cref="IAppEvent<T>"/>
        /// </summary>
        void Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event;
        event Action<EventArgs, ILogger> Disconnected;
    }
}