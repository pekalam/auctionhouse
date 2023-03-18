using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.Tests.Base;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Auctions.Tests.Base.Domain.Services.ServiceContracts;
using Chronicle;
using Common.Application;
using Common.Application.DependencyInjection;
using Common.Application.Events;
using Common.Tests.Base.Mocks;
using Common.Tests.Base.Mocks.Events;
using Core.Command.Commands.EndAuction;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using QuartzTimeTaskService.AuctionEndScheduler;
using RestEase;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TestConfigurationAccessor;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Test.Adapter.QuartzServiceAuctionEndScheduler
{
    [Trait("Category", "Integration")]
    public class QuartzAuctionEndSchedulerTests
    {
        public QuartzAuctionEndSchedulerTests()
        {
            AuctionConstantsFactoryValueProvider.TestValueFactory = (name) =>
                name switch
                {
                    nameof(AuctionConstantsFactory.MinAuctionTimeM) => 0,
                    _ => null,
                };
        }

        [Fact]
        public async Task Success_scenario_test()
        {
            var cts = new CancellationTokenSource();
            var scenario = AuctionEndSchedulerContracts.Success;
            var auctions = new FakeAuctionRepository();
            auctions.AddAuction(scenario.given.auction);
            var auctionEndScheduler = SetupTest(auctions, scenario, cts);

            var expected = await scenario.ctrl.WaitUntilEndCallIsDue(auctionEndScheduler, cts.Token);

            scenario.given.auction.Completed.Should().Be(expected.completed);
        }

        private static IAuctionEndScheduler SetupTest(FakeAuctionRepository auctions, AuctionEndSchedulerScenario scenario, CancellationTokenSource cts)
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices((services) => SetupServices(services, auctions));
                    // ... Configure test services
                });
            var server = WireMockServer.Start(TestConfig.Instance["QuartzAuctionEndSchedulerTests:TestHostUrl"]);

            var provider = SetupServices(new ServiceCollection(), auctions);
            var client = application.CreateClient();
            var respo = Response.Create()
                    .WithStatusCode(200)
                    .WithCallback(r =>
                    {
                        var content = new StringContent(JsonSerializer.Serialize(new TimeTaskRequest<AuctionEndTimeTaskValues>
                        {
                            Id = Guid.NewGuid(),
                            Values = new()
                            {
                                AuctionId = scenario.given.auction.AggregateId.Value,
                            }
                        }), Encoding.UTF8, "application/json");

                        content.Headers.Add("X-API-Key", "testk");

                        client.PostAsync("api/c/endAuction", content).GetAwaiter().GetResult();
                        cts.Cancel();
                        return new WireMock.ResponseMessage();
                    })
                    .WithBody("");


            server
            .Given(Request.Create().WithPath("/foo").UsingPost())
            .RespondWith(
                  respo
                );

            
            return provider.GetRequiredService<IAuctionEndScheduler>();
        }

        private static IServiceProvider SetupServices(IServiceCollection services, FakeAuctionRepository auctions)
        {
            services.AddLogging(b => b.AddXUnit());

            new CommonApplicationInstaller(services)
                .AddCommandCoreDependencies(
                    eventOutboxFactory: (prov) => EventOutboxMock.Instance,
                    implProviderFactory: ImplProviderMock.Factory,
                typeof(QuartzAuctionEndSchedulerTests).Assembly, typeof(EndAuctionCommandHandler).Assembly)
                .AddEventBus(_ => EventBusMock.Instance)
                .AddAppEventBuilder(_ => TestAppEventBuilder.Instance)
                .AddOutboxItemStore(_ => Mock.Of<IOutboxItemStore>());

            var settings = TimeTaskServiceSettingsFactory.Create();
            new AuctionsDomainMockInstaller(services)
                .AddAuctionRepository((prov) => auctions)
                .AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(_ => RestClient.For<ITimeTaskClient>(settings.ConnectionString), null, settings);

            services.AddChronicle(b => b.UseInMemoryPersistence());

            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}