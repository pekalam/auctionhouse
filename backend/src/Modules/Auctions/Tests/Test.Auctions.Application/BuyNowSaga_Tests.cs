using Auctions.Application.Commands.BuyNow;
using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Auctions.Tests.Base;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;
using Chronicle;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Tests.Base;
using Common.Tests.Base.Mocks;
using Common.Tests.Base.Mocks.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Auctions.Application.Tests
{
    public class BuyNowSaga_Tests
    {
        readonly FakeAuctionRepository _auctions = new();
        readonly CommandContext commandContext = CommandContext.CreateNew("test");
        readonly Guid commandId = Guid.NewGuid();
        readonly Guid correlationId = Guid.NewGuid();
        readonly Guid buyerId = Guid.NewGuid();
        readonly string paymentMethod = "test";
        readonly EventBusMock eventBus = EventBusMock.Instance;

        [Fact]
        public async Task Should_be_completed_when_valid_events_are_published()
        {

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = new UserId(this.buyerId);
            SetupServices(eventBus, auction, buyerId, out var paymentVerification, out var serviceProvider);

            await auction.Buy(buyerId, paymentMethod, paymentVerification.Object, Mock.Of<IAuctionBuyCancellationScheduler>());
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, out var ctx, out var sagaCoordinator);

            await sagaCoordinator.ProcessAsync(new AuctionBought
            {
                PaymentMethodName = paymentMethod,
                BuyerId = buyerId,
                AuctionId = auction.AggregateId,
                Price = auction.BuyNowPrice!.Value
            }, ctx);
            eventBus.PublishedEvents.Count.Should().Be(0);

            await sagaCoordinator.ProcessAsync(new UserPayments.DomainEvents.Events.V1.BuyNowPaymentConfirmed
            {
                TransactionId = auction.Buyer,
            }, ctx);
            var expectedTypes = new[]
            {
                typeof(AuctionBuyConfirmed),
            };
            eventBus.PublishedEvents.Select(e => e.GetType()).Should().ContainInOrder(expectedTypes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Should_run_compensation_actions_when_payment_failure_event_is_sent(bool concurrentScenario)
        {

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = new UserId(this.buyerId);
            SetupServices(eventBus, auction, buyerId, out var paymentVerification, out var serviceProvider);

            await auction.Buy(buyerId, paymentMethod, paymentVerification.Object, Mock.Of<IAuctionBuyCancellationScheduler>());
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, out var ctx, out var sagaCoordinator);


            await sagaCoordinator.ProcessAsync(new AuctionBought
            {
                PaymentMethodName = paymentMethod,
                BuyerId = buyerId,
                AuctionId = auction.AggregateId,
                Price = auction.BuyNowPrice!.Value
            }, ctx);
            eventBus.PublishedEvents.Count.Should().Be(0);

            if (concurrentScenario)
            {
                var user2 = UserId.New();
                auction.CancelBuy();
                var paymentVerificationForUser2 = new GivenAuctionPaymentVerification().BuildValidMock(auction, user2);
                await auction.Buy(user2, "test", paymentVerificationForUser2.Object, Mock.Of<IAuctionBuyCancellationScheduler>());
                auction.MarkPendingEventsAsHandled();
            }

            await sagaCoordinator.ProcessAsync(new UserPayments.DomainEvents.Events.V1.BuyNowPaymentFailed
            {
                TransactionId = buyerId,
            }, ctx);
            if (!concurrentScenario)
            {
                var expectedTypes = new[]
                {
                    typeof(AuctionBuyCanceled),
                };
                eventBus.PublishedEvents.Select(e => e.GetType()).Should().ContainInOrder(expectedTypes);
            }
            else
            {
                eventBus.PublishedEvents.Count.Should().Be(1);
                eventBus.PublishedEvents.First().Should().BeOfType<AuctionBuyCanceledConcurrently>();
            }
        }



        private static void SetupSagaContextAndCoordinator(ServiceProvider serviceProvider, Auction auction, Guid correlationId, CommandContext commandContext, out ISagaContext ctx, out ISagaCoordinator sagaCoordinator)
        {
            ctx = SagaContext.Create().WithSagaId(correlationId.ToString())
                .WithMetadata(BuyNowSaga.AuctionContextParamName, auction)
                .WithMetadata(BuyNowSaga.TransactionContextParamName, auction.Buyer.Value)
                .WithMetadata(BuyNowSaga.CmdContextParamName, commandContext)
                .Build();
            sagaCoordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        }

        private void SetupServices(IEventBus eventBus, Auction auction, UserId buyerId, out Mock<IAuctionPaymentVerification> paymentVerification, out ServiceProvider serviceProvider)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<EventOutboxMock>();
            new CommonApplicationMockInstaller(serviceCollection)
                .AddCommandCoreDependencies(
                    eventOutboxFactory: (prov) => prov.GetRequiredService<EventOutboxMock>(),
                    implProviderFactory: ImplProviderMock.Factory,
                    new[] { Assembly.Load("Auctions.Application") })
                .AddAppEventBuilder((prov) => TestAppEventBuilder.Instance)
                .AddEventBus((prov) => eventBus)
                .AddEventOutbox(new());

            new AuctionsDomainMockInstaller(serviceCollection)
                .AddAuctionRepository(_ => _auctions);

            serviceCollection.AddChronicle(b => b.UseInMemoryPersistence());

            paymentVerification = new GivenAuctionPaymentVerification().BuildValidMock(auction, buyerId);

            serviceCollection.AddLogging();
            //serviceCollection.AddTransient<EventBusHelper>(s => new EventBusHelper(eventBus, new TestAppEventBuilder()));
            serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
