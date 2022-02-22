﻿using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;

namespace RabbitMq.EventBus
{
    public class AppEventRabbitMQ<T> : IAppEvent<T> where T : Event
    {
        public CommandContext CommandContext { get; set; }
        public T Event { get; set; }
        public ReadModelNotificationsMode ReadModelNotifications { get; set; }
        public int RedeliveryCount { get; set; }

        public AppEventRabbitMQ(CommandContext commandContext, T @event, ReadModelNotificationsMode consistencyMode, int redeliveryCount)
        {
            CommandContext = commandContext;
            Event = @event;
            ReadModelNotifications = consistencyMode;
            RedeliveryCount = redeliveryCount;
        }
    }
}