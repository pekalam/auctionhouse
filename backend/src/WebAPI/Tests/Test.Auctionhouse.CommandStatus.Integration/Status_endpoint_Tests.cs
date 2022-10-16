using Adapter.EfCore.ReadModelNotifications;
using Auctionhouse.CommandStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TestConfigurationAccessor;
using Xunit;

namespace Test.Auctionhouse.CommandStatus.Integration
{
    [Trait("Category", "Integration")]
    public class Status_endpoint_Tests
    {
        [Fact]
        public async Task Gets_saved_status_with_valid_value()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseConfiguration(TestConfig.Instance);
                });

            var dbContext = new SagaEventsConfirmationDbContext(new DbContextOptionsBuilder<SagaEventsConfirmationDbContext>()
                .UseSqlServer(TestConfig.Instance["CommandStatusServiceOptions:ConnectionString"])
                .Options);

            var testConfirmations = new DbSagaEventsConfirmation
            {
                CommandId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
            };
            dbContext.SagaEventsConfirmations.Add(testConfirmations);
            dbContext.SaveChanges();

            var client = application.CreateClient();

            var response = await client.GetFromJsonAsync<CommandStatusDto>("api/s/status/" + testConfirmations.CommandId);
            Assert.True(response?.Status == "PENDING");
        }
    }
}