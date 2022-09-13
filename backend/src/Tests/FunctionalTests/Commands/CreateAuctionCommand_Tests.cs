using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using Auctions.Tests.Base.Domain.Services.Fakes;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
{
    [CollectionDefinition(nameof(CommandTestsCollection), DisableParallelization = true)]
    public class CommandTestsCollection { }

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class CreateAuctionCommand_Tests : TestBase
    {
        private readonly FakeAuctionRepository auctions;
        private readonly InMemoryAuctionBidsRepository auctionBids;
        private readonly ITestOutputHelper outputHelper;

        public CreateAuctionCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "ReadModel.Core")
        {
            this.outputHelper = outputHelper;
            auctions = (FakeAuctionRepository)ServiceProvider.GetRequiredService<IAuctionRepository>();
            auctionBids = (InMemoryAuctionBidsRepository)ServiceProvider.GetRequiredService<IAuctionBidsRepository>();

        }

        [Fact]
        public async Task Creates_auction_and_unlocks_it_when_pending_events_are_processed()
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(2000);

            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);
            AssertEventual(() => {
                using var scope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var _readModelNotificationsDbContext = scope.ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
                return _readModelNotificationsDbContext.SagaEventsConfirmations.FirstOrDefault() != null;
            });
            await Task.Delay(3000);



            AssertEventual(() =>
            {
                using var scope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var _readModelNotificationsDbContext = scope.ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
                var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations.FirstOrDefault()?.Completed == true;
                var confirmationEventsProcessed = _readModelNotificationsDbContext.SagaEventsToConfirm.All(e => e.Processed);
                var createdAuction = auctions.All.FirstOrDefault();
                var auctionUnlocked = createdAuction != null && !createdAuction.Locked;
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.FirstOrDefault(a => a.AuctionId.Value == createdAuction?.AggregateId.Value) is not null);
                if (!confirmationsMarkedAsCompleted) outputHelper.WriteLine("Notifications not marked as completed");
                return auctionUnlocked && idEqual && confirmationsMarkedAsCompleted && confirmationEventsProcessed && AssertReadModel(createdAuction);
            });
        }

        private bool AssertReadModel(Auctions.Domain.Auction? createdAuction)
        {
            if(createdAuction == null) return false;

            var auctionRead = ReadModelDbContext.AuctionsReadModel
                .Find(a => a.AuctionId == createdAuction.AggregateId.ToString())
                .SingleOrDefault();

            return auctionRead != null;
        }

        [Fact]
        public async Task Does_not_create_auction_if_session_is_not_created()
        {
            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await Assert.ThrowsAnyAsync<Exception>(() => Mediator.Send(createAuctionCmd));
        }
    }
}