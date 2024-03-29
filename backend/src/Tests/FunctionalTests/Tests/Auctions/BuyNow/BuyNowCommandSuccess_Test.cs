﻿using Auctions.Application.Commands.BuyNow;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static FunctionalTests.Tests.Auctions.Helpers.TestCreateAuctionCommandBuilder;
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Tests.Auctions.BuyNow
{
    using FunctionalTests.Tests.Auctions.CreateAuction;
    using global::Users.Domain.Events;
    using global::Users.DomainEvents;

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommandSuccess_Test : BuyNowCommandTestBase
    {
        readonly Type[] SuccessExpectedEvents = new[] {
                    typeof(global::Auctions.DomainEvents.Events.V1.AuctionBought),
                    typeof(UserPayments.Domain.Events.BuyNowPaymentCreated),
                    typeof(LockedFundsCreated),
                    typeof(UserCreditsLockedForBuyNowAuction),
                    typeof(BuyNowPaymentConfirmed),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToConfirmed),
                    typeof(global::Auctions.DomainEvents.Events.V1.AuctionBuyConfirmed),
                    typeof(UserPayments.Domain.Events.PaymentStatusChangedToCompleted),
                    typeof(CreditsWithdrawn),
                };

        public BuyNowCommandSuccess_Test(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task BuyNowCommand_creates_confirmed_payment_and_sets_auction_to_completed()
        {
            var createAuctionCommand = GivenCreateAuctionCommand().Build();
            var createdAuction = await CreateAuction(createAuctionCommand);
            var initialCredits = 1000m;
            var user = await CreateUser(initialCredits);
            user.MarkPendingEventsAsHandled();
            CreateUserPayments(user);

            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = Guid.Parse(createdAuction.AuctionId),
            };
            var status = await SendCommand(buyNowCommand);

            AssertEventual(new BuyNowCommandProbe(this, buyNowCommand.AuctionId, status, SuccessExpectedEvents, user, createAuctionCommand.BuyNowPrice, initialCredits).Check);
        }
    }
}
