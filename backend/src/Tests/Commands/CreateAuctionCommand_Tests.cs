using AuctionBids.Application.EventSubscriptions;
using AuctionBids.Domain.Repositories;
using Auctions.Application;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Application.EventSubscriptions;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
{
    using AuctionBids.Domain;
    using FunctionalTests.Mocks;

    public static class DiTestUtils
    {
        public static IServiceProvider CreateServiceProvider(Action<IServiceCollection> build)
        {
            var services = new ServiceCollection();
            build(services);
            return services.BuildServiceProvider();
        }
    }

    public class CreateAuctionCommand_Tests
    {
        [Fact]
        public async Task Creates_auction_and_unlocks_it_when_pending_events_are_processed()
        {
            var serviceProvider = BuildConfiguredServiceProvider();

            RabbitMqInstaller.InitializeEventSubscriptions(serviceProvider, Assembly.Load("AuctionBids.Application"),
                 Assembly.Load("Auctions.Application"));

            var commandHandler = serviceProvider.GetRequiredService<CreateAuctionCommandHandler>();
            await commandHandler.Handle(new AppCommand<CreateAuctionCommand>
            {
                Command = GivenCreateAuctionCommand().Build(),
                CommandContext = CommandContext.CreateNew(Guid.NewGuid(), "test"),
            }, CancellationToken.None);

            var auctions = (InMemoryAuctionRepository)serviceProvider.GetRequiredService<IAuctionRepository>();
            var auctionBids = (InMemoryAuctionBidsRepository)serviceProvider.GetRequiredService<IAuctionBidsRepository>();

            await Task.Delay(5000);

            var createdAuction = auctions.All.First();
            Assert.False(createdAuction.Locked);
            Assert.True(auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value,
                $"Created auctionBids aggregate should have same value of {nameof(AuctionBids.AuctionId)} as created auction");
        }

        private static IServiceProvider BuildConfiguredServiceProvider()
        {
            return DiTestUtils.CreateServiceProvider((services) =>
            {
                services.AddAuctionsModule();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
                services.AddSingleton<IAuctionBidsRepository, InMemoryAuctionBidsRepository>();
                services.AddTransient(s => () => s.GetRequiredService<IAuctionRepository>());
                services.AddTransient<IConvertCategoryNamesToRootToLeafIds, ConvertCategoryNamesToRootToLeafIdsMock>();
                services.AddTransient(s => Mock.Of<ILogger<CreateAuctionCommandHandler>>());
                services.AddTransient<CreateAuctionService>();
                services.AddTransient<IAuctionEndScheduler, AuctionEndSchedulerMock>();

                services.AddTransient<Func<ISagaNotifications>>(p => () => new InMemorySagaNotifications());

                services.AddLogging();

                services.AddRabbitMq(new RabbitMqSettings
                {
                    ConnectionString = "host=localhost"
                });

                services.AddTransient<IImplProvider>((p) => new ImplProviderMock(p));

                services.AddTransient<CreateAuctionCommandHandler>();
                services.AddTransient<AuctionCreatedSubscriber>();
                services.AddTransient<AuctionBidsCreatedSubscriber>();
            });
        }
    }
}