using Adapter.EfCore.ReadModelNotifications;
using Common.Application.Events;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.EventBus;
using System;
using System.Linq;

namespace FunctionalTests.Commands
{
    public partial class TestBase
    {
        private static void TruncateReadModelNotificaitons(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmations = dbContext.SagaEventsConfirmations.ToList();
            dbContext.SagaEventsConfirmations.RemoveRange(confirmations);

            var eventsToConfirm = dbContext.SagaEventsToConfirm.ToList();
            dbContext.SagaEventsToConfirm.RemoveRange(eventsToConfirm);

            dbContext.SaveChanges();
        }

        public virtual void Dispose()
        {
            _modelUserReadTestHelper.Dispose();
            ServiceProvider.GetRequiredService<IEasyMQBusInstance>().Dispose();
            TruncateReadModelNotificaitons(ServiceProvider);
        }
    }
}
