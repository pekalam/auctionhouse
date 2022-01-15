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
using static UserPayments.DomainEvents.Events.V1;

namespace FunctionalTests.Commands
{
    using UserPayments.Domain;
    using UserPayments.Domain.Events;
    using UserPayments.Domain.Shared;

    [Collection(nameof(CommandTestsCollection))]
    [Trait("Category", "Functional")]
    public class BuyNowCommand_Tests : TestBase, IDisposable
    {
        private readonly InMemoryAuctionRepository auctions;
        private readonly InMemoryAuctionBidsRepository auctionBids;
        private readonly InMemortUserPaymentsRepository userPayments;
        private readonly ITestOutputHelper outputHelper;

        public BuyNowCommand_Tests(ITestOutputHelper outputHelper) : base(outputHelper, "AuctionBids.Application", "Auctions.Application", "UserPayments.Application")
        {
            this.outputHelper = outputHelper;
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
            await Task.Delay(3000);

            //create auction in session
            var createAuctionCmd = GivenCreateAuctionCommand().Build();
            await SendCommand(createAuctionCmd);
            await Task.Delay(3000);


            AssertEventual(() =>
            {
                var createdAuction = auctions.All.First();
                var auctionLocked = (createdAuction.Locked);
                var idEqual = (auctionBids.All.Count > 0 && auctionBids.All.First().AuctionId.Value == createdAuction.AggregateId.Value);
                return !auctionLocked && idEqual;
            });
            InMemoryEventBusDecorator.ClearSentEvents();

            var userId = Guid.NewGuid();
            ChangeSignedInUser(userId);
            userPayments.Add(UserPayments.CreateNew(new UserId(userId)));
            var buyNowCommand = new BuyNowCommand
            {
                AuctionId = auctions.All.First().AggregateId,
            };
            await SendCommand(buyNowCommand);

            AssertEventual(() =>
            {
                var payment = userPayments.All.FirstOrDefault(u => u.UserId == userId)
                        ?.Payments.FirstOrDefault(p => p.PaymentTargetId?.Value == buyNowCommand.AuctionId);
                var auction = auctions.All.FirstOrDefault(a => a.AggregateId == buyNowCommand.AuctionId);
                var allEventsPublished = AssertExpectedEventsArePublished();

                return allEventsPublished && payment?.Status == PaymentStatus.Completed &&
                    auction is not null && auction.Completed;
            });
        }

        private bool AssertExpectedEventsArePublished()
        {
            var expectedEvents = new[] {
                    typeof(Auctions.DomainEvents.AuctionLocked),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXStarted),
                    typeof(Auctions.DomainEvents.Events.V1.BuyNowTXSuccess),
                    typeof(UserPaymentsCreated),
                    typeof(BuyNowPaymentCreated),
                    typeof(PaymentStatusChangedToConfirmed),
                    typeof(BuyNowPaymentConfirmed),
                    typeof(PaymentStatusChangedToCompleted),
                };

            var allEventsPublished = SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents).Any() == false;

            if (SentEvents.Count > expectedEvents.Length)
            {
                outputHelper.WriteLine("Not all events were included in expected");
                foreach (var ev in SentEvents.Select(e => e.Event.GetType()).Except(expectedEvents))
                {
                    outputHelper.WriteLine("Event: " + ev.Name);
                }
            }
            else if (!allEventsPublished)
            {
                var notPublished = expectedEvents.Except(SentEvents.Select(e => e.Event.GetType()));
                outputHelper.WriteLine($"Not all expected events were published ({notPublished.Count()}/{expectedEvents.Length}):");
                foreach (var ev in notPublished)
                {
                    outputHelper.WriteLine("Not published: " + ev.Name);
                }
            }
            else
            {
                outputHelper.WriteLine("All events were published");
            }

            return allEventsPublished;
        }

        public void Dispose()
        {
            TruncateReadModelNotificaitons(ServiceProvider);
        }

    }
}
