using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.Domain;
using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using Auctions.Tests.Base.Domain.Services.Fakes;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Core.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
{
    public class CreateAuctionProbe
    {
        private readonly TestBase _testBase;

        public CreateAuctionProbe(TestBase testBase)
        {
            _testBase = testBase;
        }

        public bool Check()
        {
            using var scope = _testBase.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var auctions = (FakeAuctionRepository)_testBase.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var auctionBids = (InMemoryAuctionBidsRepository)_testBase.ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            var _readModelNotificationsDbContext = scope.ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations.FirstOrDefault()?.Completed == true;
            var confirmationEventsProcessed = _readModelNotificationsDbContext.SagaEventsToConfirm.All(e => e.Processed);
            var createdAuction = auctions.All.FirstOrDefault();
            var auctionUnlocked = createdAuction != null && !createdAuction.Locked;
            var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.FirstOrDefault(a => a.AuctionId.Value == createdAuction?.AggregateId.Value) is not null);
            //if (!confirmationsMarkedAsCompleted) outputHelper.WriteLine("Notifications not marked as completed");
            return auctionUnlocked && idEqual && confirmationsMarkedAsCompleted && confirmationEventsProcessed && AssertReadModel(createdAuction);
        }

        private bool AssertReadModel(Auctions.Domain.Auction? createdAuction)
        {
            if (createdAuction == null) return false;

            var auctionRead = _testBase.ReadModelDbContext.AuctionsReadModel
                .Find(a => a.AuctionId == createdAuction.AggregateId.ToString())
                .SingleOrDefault();

            return auctionRead != null;
        }
    }

    [CollectionDefinition(nameof(CommandTestsCollection), DisableParallelization = true)]
    public class CommandTestsCollection { }

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class CreateAuctionCommand_Tests : TestBase
    {
        private readonly ITestOutputHelper outputHelper;

        public CreateAuctionCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "ReadModel.Core")
        {
            this.outputHelper = outputHelper;
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

            AssertEventual(new CreateAuctionProbe(this).Check);
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