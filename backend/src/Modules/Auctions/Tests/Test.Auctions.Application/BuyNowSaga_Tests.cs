using AuctionBids.DomainEvents;
using Auctions.Application.Commands.BuyNow;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Mediator;
using Common.Application.SagaNotifications;
using Common.Tests.Base.Mocks;
using Common.Tests.Base.Mocks.Events;
using Core.Common.Domain;
using FluentAssertions;
using FluentAssertions.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Auctions.DomainEvents.Events.V1;
using Auctions.Tests.Base.Domain.Services.Fakes;
using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.Services.TestDoubleBuilders;

namespace Auctions.Application.Tests
{
    public class BuyNowSaga_Tests
    {
        FakeAuctionRepository _auctions = new();
        CommandContext commandContext = CommandContext.CreateNew("test");
        Guid commandId = Guid.NewGuid();
        Guid correlationId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        string paymentMethod = "test";
        EventBusMock eventBus = EventBusMock.Instance;

        [Fact]
        public async Task Should_be_completed_when_valid_events_are_published()
        {

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = new UserId(this.buyerId);
            SetupServices(eventBus, auction, buyerId, out var paymentVerification, out var serviceProvider);

            await auction.Buy(buyerId, paymentMethod, paymentVerification.Object, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId = auction.TransactionId;
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, transactionId, out var ctx, out var sagaCoordinator);

            await sagaCoordinator.ProcessAsync(new BuyNowTXStarted
            {
                PaymentMethodName = paymentMethod,
                BuyerId = buyerId,
                AuctionId = auction.AggregateId,
                TransactionId = transactionId.Value,
                Price = auction.BuyNowPrice!.Value
            }, ctx);
            eventBus.PublishedEvents.Count.Should().Be(0);

            await sagaCoordinator.ProcessAsync(new UserPayments.DomainEvents.Events.V1.BuyNowPaymentConfirmed
            {
                TransactionId = transactionId.Value,
            }, ctx);
            eventBus.PublishedEvents.Count.Should().Be(2);
            var expectedTypes = new[]
            {
                typeof(BuyNowTXSuccess),
                typeof(AuctionUnlocked),
            };
            foreach (var messageType in eventBus.PublishedEvents.Select(e => e.GetType()))
            {
                expectedTypes.Contains(messageType).Should().BeTrue();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Should_run_compensation_actions_when_payment_failure_event_is_sent(bool concurrentScenario)
        {

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var buyerId = new UserId(this.buyerId);
            SetupServices(eventBus, auction, buyerId, out var paymentVerification, out var serviceProvider);

            await auction.Buy(buyerId, paymentMethod, paymentVerification.Object, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId = auction.TransactionId;
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, transactionId, out var ctx, out var sagaCoordinator);


            await sagaCoordinator.ProcessAsync(new BuyNowTXStarted
            {
                PaymentMethodName = paymentMethod,
                BuyerId = buyerId,
                AuctionId = auction.AggregateId,
                TransactionId = transactionId.Value,
                Price = auction.BuyNowPrice!.Value
            }, ctx);
            eventBus.PublishedEvents.Count.Should().Be(0);

            if (concurrentScenario)
            {
                var user2 = UserId.New();
                auction.Unlock(auction.LockIssuer);
                var paymentVerificationForUser2 = new GivenAuctionPaymentVerification().BuildValidMock(auction, user2);
                await auction.Buy(user2, "test", paymentVerificationForUser2.Object, Mock.Of<IAuctionUnlockScheduler>());
                auction.MarkPendingEventsAsHandled();
            }

            await sagaCoordinator.ProcessAsync(new UserPayments.DomainEvents.Events.V1.BuyNowPaymentFailed
            {
                TransactionId = transactionId.Value,
            }, ctx);
            if (!concurrentScenario)
            {
                eventBus.PublishedEvents.Count.Should().Be(2);
                var expectedTypes = new[]
                {
                    typeof(BuyNowTXCanceled),
                    typeof(AuctionUnlocked),
                };
                foreach (var messageType in eventBus.PublishedEvents.Select(e => e.GetType()))
                {
                    expectedTypes.Contains(messageType).Should().BeTrue();
                }
            }
            else
            {
                eventBus.PublishedEvents.Count.Should().Be(1);
                eventBus.PublishedEvents.First().Should().BeOfType<BuyNowTXCanceledConcurrently>();
            }
        }



        private static void SetupSagaContextAndCoordinator(ServiceProvider serviceProvider, Auction auction, Guid correlationId, CommandContext commandContext, Guid? transactionId, out ISagaContext ctx, out ISagaCoordinator sagaCoordinator)
        {
            ctx = SagaContext.Create().WithSagaId(correlationId.ToString())
                .WithMetadata(BuyNowSaga.AuctionContextParamName, auction)
                .WithMetadata(BuyNowSaga.TransactionContextParamName, transactionId)
                .WithMetadata(BuyNowSaga.CmdContextParamName, commandContext)
                .Build();
            sagaCoordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        }

        private void SetupServices(IEventBus eventBus, Auction auction, UserId buyerId, out Mock<IAuctionPaymentVerification> paymentVerification, out ServiceProvider serviceProvider)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCommonCommandDependencies(new[] { Assembly.Load("Auctions.Application") });

            serviceCollection.AddChronicle(b => b.UseInMemoryPersistence());
            paymentVerification = new GivenAuctionPaymentVerification().BuildValidMock(auction, buyerId);
            serviceCollection.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            serviceCollection.AddSingleton<IAuctionRepository>(_auctions);
            serviceCollection.AddTransient(s => Mock.Of<IAuctionUnlockScheduler>());
            serviceCollection.AddScoped<EventOutboxMock>();
            serviceCollection.AddTransient<IEventOutbox, EventOutboxMock>(s => s.GetRequiredService<EventOutboxMock>());
            serviceCollection.AddTransient<IEventOutboxSavedItems, EventOutboxMock>(s => s.GetRequiredService<EventOutboxMock>());
            serviceCollection.AddTransient(s => UnitOfWorkFactoryMock.Instance.Object);
            serviceCollection.AddTransient(s => Mock.Of<ISagaNotifications>());
            serviceCollection.AddTransient<OptimisticConcurrencyHandler>();

            serviceCollection.AddSingleton(eventBus);
            serviceCollection.AddTransient<IAppEventBuilder, TestAppEventBuilder>();
            serviceCollection.AddTransient<EventOutboxSender>();
            serviceCollection.AddTransient(s => Mock.Of<IOutboxItemStore>());

            serviceCollection.AddLogging();
            //serviceCollection.AddTransient<EventBusHelper>(s => new EventBusHelper(eventBus, new TestAppEventBuilder()));
            serviceProvider = serviceCollection.BuildServiceProvider();

        }
    }

    internal class EventOutboxMock : IEventOutbox, IEventOutboxSavedItems
    {
        private readonly List<OutboxItem> _outboxItems = new();

        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _outboxItems;

        public Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var item = new OutboxItem
            {
                CommandContext = commandContext,
                Event = @event,
                ReadModelNotifications = notificationsMode,
            };
            _outboxItems.Add(item);
            return Task.FromResult(item);
        }

        public Task<OutboxItem[]> SaveEvents(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var outboxItems = events.Select(e => new OutboxItem
            {
                CommandContext = commandContext,
                Event = e,
                ReadModelNotifications = notificationsMode,
            }).ToArray();
            _outboxItems.AddRange(outboxItems);
            return Task.FromResult(outboxItems);
        }
    }
}
