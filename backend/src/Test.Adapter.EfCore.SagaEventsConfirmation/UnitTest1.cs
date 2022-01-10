using Adapter.EfCore.ReadModelNotifications;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;

namespace Test.Adapter.EfCore.ReadModelNotifications
{
    internal class TestEvent : Event
    {
        public TestEvent(string eventName) : base(eventName)
        {
        }
    }

    public class UnitTest1
    {
        SagaEventsConfirmationDbContext dbContext;
        EfCoreSagaNotifications sagaEventNotifications;

        public UnitTest1()
        {
            dbContext = new SagaEventsConfirmationDbContext(new DbContextOptionsBuilder<SagaEventsConfirmationDbContext>()
                .UseInMemoryDatabase("testDb").Options);
            sagaEventNotifications = new EfCoreSagaNotifications(dbContext);
        }

        [Fact]
        public async Task Test1()
        {
            var correlationId = CorrelationId.CreateNew();
            var commandId = CommandId.CreateNew();
            await sagaEventNotifications.RegisterNewSaga(correlationId, commandId);

            Assert.NotNull(dbContext.SagaEventsConfirmations.FirstOrDefaultAsync(e => e.CorrelationId == correlationId.Value));
        }

        [Fact]
        public async Task Test2()
        {
            var correlationId = CorrelationId.CreateNew();
            var commandId = CommandId.CreateNew();
            await sagaEventNotifications.RegisterNewSaga(correlationId, commandId);

            await sagaEventNotifications.AddUnhandledEvent(correlationId, new TestEvent("event1"));
            await sagaEventNotifications.AddUnhandledEvent(correlationId, new TestEvent("event2"));

            var dbConfirmations = await dbContext.SagaEventsConfirmations
                .FirstAsync(e => e.CorrelationId == correlationId.Value);

            Assert.Equal("event1,event2", dbConfirmations.UnprocessedEvents);
        }
    }
}