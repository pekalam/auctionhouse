using Adapter.QuartzTimeTaskService.AuctionEndScheduler;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Mediator;
using Core.Command.Commands.EndAuction;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using QuartzTimeTaskService.AuctionEndScheduler;
using RestEase;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Test.Auctions.Base.Mocks;
using Test.Auctions.Base.ServiceContracts;
using Test.Common.Base.Mocks;
using Test.Common.Base.Mocks.Events;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Test.Adapter.QuartzServiceAuctionEndScheduler
{
    internal class ImplProviderMock : IImplProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ImplProviderMock(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Get<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        public object Get(Type t)
        {
            return _serviceProvider.GetService(t);
        }
    }

    public class TimeTaskServiceSettingsFactory
    {
        public static string Url { get; set; }

        public static TimeTaskServiceSettings Create()
        {
            return new TimeTaskServiceSettings
            {
                ConnectionString = "http://localhost:5001",
                ApiKey = "testk",
                AuctionEndEchoTaskEndpoint = Url.Replace("localhost", "host.docker.internal") + "/foo",
            };
        }
    }

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
            var auctions = new InMemoryAuctionRepository();
            auctions.AddAuction(scenario.given.auction);
            var auctionEndScheduler = SetupTest(auctions, scenario, cts);
            
            var expected = await scenario.ctrl.WaitUntilEndCallIsDue(auctionEndScheduler, cts.Token);

            scenario.given.auction.Completed.Should().Be(expected.completed);
        }

        private static QuartzAuctionEndScheduler SetupTest(InMemoryAuctionRepository auctions, AuctionEndSchedulerScenario scenario, CancellationTokenSource cts)
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {

                    builder.ConfigureServices((services) => SetupServices(services, auctions));
                    // ... Configure test services
                });
            var server = WireMockServer.Start();
            var url = server.Urls[0];
            TimeTaskServiceSettingsFactory.Url = url;

            SetupServices(new ServiceCollection(), auctions);
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

            var settings = TimeTaskServiceSettingsFactory.Create();
            return new QuartzAuctionEndScheduler(RestClient.For<ITimeTaskClient>(settings.ConnectionString), settings);
        }

        private static IServiceProvider SetupServices(IServiceCollection services, InMemoryAuctionRepository auctions)
        {
            services.AddTransient<IRequestHandler<AppCommand<EndAuctionCommand>, RequestStatus>, EndAuctionCommandHandler>();
            services.AddSingleton<IAuctionRepository>(auctions);
            services.AddLogging(b => b.AddXUnit());
            services.AddSingleton<IEventBus>(EventBusMock.Instance);
            services.AddTransient<IAppEventBuilder, TestAppEventBuilder>();
            services.AddTransient(s => Mock.Of<IOutboxItemStore>());
            services.AddTransient<IImplProvider, ImplProviderMock>();
            
            services.AddConcurrencyUtils();
            services.AddCommandHandling<EventOutboxMock>(typeof(QuartzAuctionEndSchedulerTests).Assembly);
            services.AddTransient(s => TimeTaskServiceSettingsFactory.Create());
            AttributeStrategies.LoadCommandAttributeStrategies(Assembly.Load("Auctions.Application"));
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}