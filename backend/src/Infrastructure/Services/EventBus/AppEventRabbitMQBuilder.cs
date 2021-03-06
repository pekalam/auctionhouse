﻿using Core.Command;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;

namespace Infrastructure.Services.EventBus
{
    public class AppEventRabbitMQBuilder : IAppEventBuilder
    {
        private CorrelationId _correlationId;
        private Event _event;
        private CommandBase _commandBase;

        public IAppEventBuilder WithCommand(CommandBase cmd)
        {
            _commandBase = cmd;
            return this;
        }

        public IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event
        {
            _event = @event;
            return this;
        }

        public IAppEventBuilder WithCorrelationId(CorrelationId correlationId)
        {
            _correlationId = correlationId;
            return this;
        }

        public IAppEvent<TEvent> Build<TEvent>() where TEvent : Event
        {
            return new AppEventRabbitMQ<TEvent>(_correlationId, (TEvent)_event, _commandBase);
        }
    }
}