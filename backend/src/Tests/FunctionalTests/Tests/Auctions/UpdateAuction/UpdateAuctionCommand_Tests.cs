using Adapter.EfCore.ReadModelNotifications;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Application.Commands.UpdateAuction;
using FunctionalTests.Commands;
using FunctionalTests.Tests.Auctions.CreateAuction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Core.Model;
using ReadModel.Core.Queries.User.UserAuctions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Tests.Auctions.Helpers.TestCreateAuctionCommandBuilder;

namespace FunctionalTests.Tests.Auctions.UpdateAuction
{
    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class UpdateAuctionCommand_Tests : TestBase
    {
        private readonly ITestOutputHelper outputHelper;

        public UpdateAuctionCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "ReadModel.Core")
        {
            this.outputHelper = outputHelper;
        }


        [Fact]
        public async Task Updates_created_auction()
        {
            //start session
            await StartAuctionCreateSession();
            //create auction in session
            await CreateAuction();
            var createdAuctionRead = WaitForCreatedAuctionRead();

            var updateAuctionCommand = new UpdateAuctionCommand
            {
                AuctionId = Guid.Parse(createdAuctionRead.AuctionId),
                BuyNowPrice = createdAuctionRead.BuyNowPrice + 1, //TODO test all props
                EndDate = null,
            };
            var requestStatus = await SendCommand(updateAuctionCommand);

            AssertEventual(() =>
            {
                //assert confirmations
                var _readModelNotificationsDbContext = ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
                var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CommandId == requestStatus.CommandId.Id)?.Completed == true;

                // assert read model changes
                var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, createdAuctionRead.AuctionId.ToString());
                var auctionRead = ReadModelDbContext.AuctionsReadModel.FindSync(filter).First();
                return updateAuctionCommand.BuyNowPrice == auctionRead.BuyNowPrice && confirmationsMarkedAsCompleted;
            });
        }

        private async Task StartAuctionCreateSession()
        {
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(2000);
        }

        private async Task CreateAuction()
        {
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);
            await Task.Delay(2000);
        }

        private AuctionRead? WaitForCreatedAuctionRead()
        {
            AuctionRead? createdAuctionRead = null;
            AssertEventual(() =>
            {
                createdAuctionRead = SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery()).GetAwaiter().GetResult().Auctions.FirstOrDefault();
                var auctionReadUnlocked = createdAuctionRead != null && !createdAuctionRead.Locked;
                return createdAuctionRead != null && auctionReadUnlocked;
            });
            return createdAuctionRead;
        }
    }
}
