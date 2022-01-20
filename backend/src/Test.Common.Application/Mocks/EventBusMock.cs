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

    internal static class OutboxItemFinderMock
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
}
