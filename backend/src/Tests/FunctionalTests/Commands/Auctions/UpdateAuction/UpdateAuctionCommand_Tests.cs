using Adapter.EfCore.ReadModelNotifications;
using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Application.Commands.UpdateAuction;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Tests.Base.Domain.Services.Fakes;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ReadModel.Core.Model;
using ReadModel.Core.Queries.User.UserAuctions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Commands.TestCreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
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
            var startSessionCmd = new StartAuctionCreateSessionCommand();
            await SendCommand(startSessionCmd);
            await Task.Delay(2000);

            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);
            await Task.Delay(2000);

            AuctionRead? createdAuction = null!;
            AssertEventual(() =>
            {
                createdAuction = SendQuery<UserAuctionsQuery, UserAuctionsQueryResult>(new UserAuctionsQuery()).GetAwaiter().GetResult().Auctions.FirstOrDefault();
                var auctionUnlocked = createdAuction != null && !createdAuction.Locked;
                return createdAuction != null && auctionUnlocked;
            });

            var updateAuctionCommand = new UpdateAuctionCommand
            {
                AuctionId = Guid.Parse(createdAuction.AuctionId),
                BuyNowPrice = createdAuction.BuyNowPrice + 1, //TODO test all props
                EndDate = null,
            };
            var requestStatus = await SendCommand(updateAuctionCommand);
            await Task.Delay(2000);

            AssertEventual(() =>
            {
                //assert confirmations
                var _readModelNotificationsDbContext = ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
                var confirmationsMarkedAsCompleted = _readModelNotificationsDbContext.SagaEventsConfirmations
                    .FirstOrDefault(c => c.CommandId == requestStatus.CommandId.Id)?.Completed == true;

                // assert read model changes
                var filter = Builders<AuctionRead>.Filter.Eq(f => f.AuctionId, createdAuction.AuctionId.ToString());
                var auctionRead = ReadModelDbContext.AuctionsReadModel.FindSync(filter).First();
                return updateAuctionCommand.BuyNowPrice == auctionRead.BuyNowPrice && confirmationsMarkedAsCompleted;
            });
        }
    }
}
