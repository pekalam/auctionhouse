using Auctions.Application.Commands.StartAuctionCreateSession;
using FunctionalTests.Commands;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Tests.Auctions.Helpers.TestCreateAuctionCommandBuilder;

namespace FunctionalTests.Tests.Auctions.CreateAuction
{
    [CollectionDefinition(nameof(CommandTestsCollection), DisableParallelization = true)]
    public class CommandTestsCollection { }

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class CreateAuctionCommand_Tests : TestBase
    {
        private readonly ITestOutputHelper outputHelper;

        public CreateAuctionCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "Users.Application", "ReadModel.Core")
        {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public async Task Creates_auction_and_unlocks_read_model_when_pending_events_are_processed()
        {
            //start session
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(2000);

            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            var createRequestStatus = await SendCommand(createAuctionCmd);

            AssertEventual(new CreateAuctionProbe(this, createRequestStatus).Check);
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