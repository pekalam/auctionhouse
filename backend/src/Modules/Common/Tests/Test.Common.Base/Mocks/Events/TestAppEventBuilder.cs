﻿using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Base.Mocks.Events
{
    public class TestAppEvent<T> : IAppEvent<T> where T : Event
    {
        public CommandContext CommandContext { get; set; }

        public T Event { get; set; }

        public int RedeliveryCount { get; set; }
    }

    public class TestAppEventBuilder : IAppEventBuilder
    {
        public static TestAppEventBuilder Instance { get; } = new TestAppEventBuilder();

        private CommandContext _commandContext;
        private Event _event;
        private int _redeliveryCount;

        public IAppEvent<TEvent> Build<TEvent>() where TEvent : Event
        {
            return new TestAppEvent<TEvent>
            {
                Event = (TEvent)_event,
                CommandContext = _commandContext,
                RedeliveryCount = _redeliveryCount,
            };
        }

        public IAppEventBuilder WithCommandContext(CommandContext commandContext)
        {
            _commandContext = commandContext;
            return this;
        }

        public IAppEventBuilder WithEvent<TEvent>(TEvent @event) where TEvent : Event
        {
            _event = @event;
            return this;
        }

        public IAppEventBuilder WithRedeliveryCount(int redeliveryCount)
        {
            _redeliveryCount = redeliveryCount;
            return this;
        }
    }
}
