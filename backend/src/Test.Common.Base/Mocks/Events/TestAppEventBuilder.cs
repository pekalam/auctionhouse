using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common.Base.Mocks.Events
{
    public class TestAppEvent<T> : IAppEvent<T> where T : Event
    {
        public CommandContext CommandContext { get; set; }

        public T Event { get; set; }

        public ReadModelNotificationsMode ReadModelNotifications { get; set; }
    }

    public class TestAppEventBuilder : IAppEventBuilder
    {
        private CommandContext _commandContext;
        private ReadModelNotificationsMode _readModelNotificationsMode;
        private Event _event;

        public IAppEvent<TEvent> Build<TEvent>() where TEvent : Event
        {
            return new TestAppEvent<TEvent>
            {
                Event = (TEvent)_event,
                CommandContext = _commandContext,
                ReadModelNotifications = _readModelNotificationsMode,
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

        public IAppEventBuilder WithReadModelNotificationsMode(ReadModelNotificationsMode consistencyMode)
        {
            _readModelNotificationsMode = consistencyMode;
            return this;
        }
    }
}
