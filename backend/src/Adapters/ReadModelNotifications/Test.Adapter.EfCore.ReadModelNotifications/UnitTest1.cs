using Adapter.EfCore.ReadModelNotifications;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
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
            var services = new ServiceCollection();
            dbContext = new SagaEventsConfirmationDbContext(new DbContextOptionsBuilder<SagaEventsConfirmationDbContext>()
                .UseInMemoryDatabase("testDb").Options);
            services.AddScoped<SagaEventsConfirmationDbContext>(s => new SagaEventsConfirmationDbContext(new DbContextOptionsBuilder<SagaEventsConfirmationDbContext>()
                .UseInMemoryDatabase("testDb").Options));
            var provider = services.BuildServiceProvider();
            
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

            var eventConfirmations = dbContext.SagaEventsToConfirm
                .Where(e => e.CorrelationId == correlationId.Value).ToArray();

            Assert.Equal(2, eventConfirmations.Length);
            Assert.Equal("event1", eventConfirmations[0].EventName);
            Assert.False(eventConfirmations[0].Processed);
            Assert.Equal("event2", eventConfirmations[1].EventName);
            Assert.False(eventConfirmations[1].Processed);
        }

        [Fact]
        public async Task Test3()
        {
            var correlationId = CorrelationId.CreateNew();
            var commandId = CommandId.CreateNew();
            await sagaEventNotifications.RegisterNewSaga(correlationId, commandId);

            var event1 = new TestEvent("event1");
            await sagaEventNotifications.AddUnhandledEvent(correlationId, new TestEvent("event1"));

            await sagaEventNotifications.MarkEventAsHandled(correlationId, event1);

            var event1Confirmation = dbContext.SagaEventsToConfirm.First(e => e.CorrelationId == correlationId.Value);
            
            Assert.True(event1Confirmation.Processed);
        }


        [Fact]
        public async Task Test4()
        {
            var correlationId = CorrelationId.CreateNew();
            var commandId = CommandId.CreateNew();
            await sagaEventNotifications.RegisterNewSaga(correlationId, commandId);
            
            await sagaEventNotifications.MarkSagaAsCompleted(correlationId);

            var event1Confirmation = dbContext.SagaEventsConfirmations.First(e => e.CorrelationId == correlationId.Value);

            Assert.True(event1Confirmation.Completed);
        }
    }
}