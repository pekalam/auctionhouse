using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
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
    public class CreateAuctionCommand_Tests : TestBase
    {
        private readonly InMemoryAuctionRepository auctions;
        private readonly InMemoryAuctionBidsRepository auctionBids;

        public CreateAuctionCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application")
        {
            auctions = (InMemoryAuctionRepository)ServiceProvider.GetRequiredService<IAuctionRepository>();
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
            await Task.Delay(2000);


            AssertEventual(() =>
            {
                var createdAuction = auctions.All.First();
                var auctionLocked = (createdAuction.Locked);
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.FirstOrDefault(a => a.AuctionId.Value == createdAuction.AggregateId.Value) is not null);
                return !auctionLocked && idEqual;
            });
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