using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common.Application.ServiceContracts;

namespace Test.Common.Application.Mocks
{
    internal static class EventBusMock
    {
        public static Mock<IEventBus> Instance
        {
            get
            {
                var mock = new Mock<IEventBus>();
                return mock;
            }
        }
    }

    internal static class OutboxItemStoreMock
    {
        public static Mock<IOutboxItemStore> Instance => new Mock<IOutboxItemStore>();
    }

    internal static class PostProcessOutboxItemServiceMock
    {
        public static Mock<IOutboxItemFinder> Instance(OutboxItemFinderContract_GetUnprocessedItemsOlder contract_getUnprocessedItemsOlder,
            OutboxItemFinderContract_GetTotalUnprocessedItemsOlder contract_getTotalUnprocessedOlder)
        {
            var mock = new Mock<IOutboxItemFinder>();

            mock.Setup(f => f.GetUnprocessedItemsOlderThan(
                It.Is<long>(n => n == contract_getUnprocessedItemsOlder.Given.diff),
                It.Is<long>(n => n == contract_getUnprocessedItemsOlder.Given.currentTimestamp),
                It.Is<int>(n => n == contract_getUnprocessedItemsOlder.Given.limit)))
                .Returns(Task.FromResult(contract_getUnprocessedItemsOlder.Expected));

            mock.Setup(f => f.GetTotalUnprocessedItemsOlderThan(
                It.Is<long>(n => n == contract_getTotalUnprocessedOlder.Given.diff),
                It.Is<long>(n => n == contract_getTotalUnprocessedOlder.Given.currentTimestamp)))
                .Returns(contract_getTotalUnprocessedOlder.Expected);

            return mock;
        }
    }

    public class TestAppEvent<T> : IAppEvent<T> where T : Event
    {
        public CommandContext CommandContext { get; set; }

        public T Event { get; set; }

        public ReadModelNotificationsMode ReadModelNotifications { get; set; }
    }

    internal class TestAppEventBuilder : IAppEventBuilder
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
