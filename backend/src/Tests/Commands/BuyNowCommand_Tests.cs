using AuctionBids.Domain.Repositories;
using Auctions.Application.Commands.BuyNow;
using Auctions.Application.Commands.StartAuctionCreateSession;
using Auctions.Domain.Repositories;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Builders.CreateAuctionCommandBuilder;

namespace FunctionalTests.Commands
{
    using UserPayments.Domain;
    using UserPayments.Domain.Shared;

    [Collection(nameof(CommandTestsCollection))]
    public class BuyNowCommand_Tests : TestBase, IDisposable
    {
        private readonly InMemoryAuctionRepository auctions;
        private readonly InMemoryAuctionBidsRepository auctionBids;
        private readonly InMemortUserPaymentsRepository userPayments;

        public BuyNowCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application")
        {
            auctions = (InMemoryAuctionRepository)ServiceProvider.GetRequiredService<IAuctionRepository>();
            auctionBids = (InMemoryAuctionBidsRepository)ServiceProvider.GetRequiredService<IAuctionBidsRepository>();
            userPayments = (InMemortUserPaymentsRepository)ServiceProvider.GetRequiredService<IUserPaymentsRepository>();
        }

        [Fact]
        public async Task BuyNowCommand_creates_confirmed_payment_and_sets_auction_to_completed()
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
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value);
                return !auctionLocked && idEqual;
            });

            var userId = Guid.NewGuid();
            ChangeSignedInUser(userId);
            userPayments.Add(UserPayments.CreateNew(new UserId(userId)));
            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First().AggregateId,
            };
            await SendCommand(buyNowCommand);

            AssertEventual(() => {
                var payment = userPayments.All.FirstOrDefault(u => u.UserId == userId)
                        ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == buyNowCommand.AuctionId);
                var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
                return payment?.Status == PaymentStatus.Confirmed && 
                    auction is not null && auction.Completed;
            });
        }

        public void Dispose()
        {
            TruncateReadModelNotificaitons(ServiceProvider);
        }

    }
}
