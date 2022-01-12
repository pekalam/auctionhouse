using Adapter.EfCore.ReadModelNotifications;
using Auctionhouse.CommandStatus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Test.Auctionhouse.CommandStatus.Integration
{
    public class UnitTest1
    {
        [Fact]
        public async Task Gets_saved_status_with_valid_value()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((ctx, configBuilder) =>
                    {
                    });
                    
                    builder.ConfigureServices(services =>
                    {

                        services.AddSingleton(new CommandStatusServiceOptions
                        {
                            ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Marek\\source\\repos\\Csharp\\auctionhouse\\backend\\src\\Tests\\FunctionalTestsServer.mdf;Integrated Security=True",
                        });
                    });
                    // ... Configure test services
                });

            var dbContext = new SagaEventsConfirmationDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Marek\\source\\repos\\Csharp\\auctionhouse\\backend\\src\\Tests\\FunctionalTestsServer.mdf;Integrated Security=True")
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