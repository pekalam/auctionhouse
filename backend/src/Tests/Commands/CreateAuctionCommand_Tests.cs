using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain.Repositories;
using Common.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
{
    using AuctionBids.Domain;
    using AuctionBids.Domain.Shared;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using FunctionalTests.Mocks;

    public class CreateAuctionCommand_Tests : TestBase
    {
        private InMemoryAuctionRepository auctions;
        private InMemoryAuctionBidsRepository auctionBids;

        public CreateAuctionCommand_Tests() : base("AuctionBids.Application", "Auctions.Application")
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

            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);


            AssertEventual(() => {
                var createdAuction = auctions.All.First();
                var auctionLocked = (createdAuction.Locked);
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value);
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