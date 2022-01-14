using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common.Base.Builders
{
    public class TestEvent : Event
    {
        public TestEvent() : base("testEvent")
        {
        }
    }

    public class GivenOutboxStoreItem
    {
        private CommandContext _commandContext = CommandContext.CreateNew("test");
        private ReadModelNotificationsMode _readModelNotificationsMode;
        private long _timestamp = 1;
        private Event _event = new TestEvent();

        public GivenOutboxStoreItem WithTimestamp(long timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public OutboxItem Build()
        {
            return new OutboxItem
            {
                CommandContext = _commandContext,
                Event = _event,
                ReadModelNotifications = _readModelNotificationsMode,
                Timestamp = _timestamp,
            };
        }
    }
}
