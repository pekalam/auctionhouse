﻿using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Common.Application.Events
{
    public interface IEventBus
    {
        /// <summary>
        /// Sends event to queue named like <see cref="IAppEvent<T>"/>
        /// </summary>
        Task Publish<T>(IAppEvent<T> @event) where T : Event;
        /// <summary>
        /// Sends event to queue named like each <see cref="IAppEvent<T>"/>
        /// </summary>
        Task Publish<T>(IEnumerable<IAppEvent<T>> events) where T : Event;
    }
}