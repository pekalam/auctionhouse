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
using Test.Auctions.Base.Builders;
using Test.Auctions.Base.Mocks;
using Test.Common.Base.Mocks;
using Test.Common.Base.Mocks.Events;
using Xunit;
using static Auctions.DomainEvents.Events.V1;

namespace Test.Auctions.Application
{
    public class BuyNowSaga_Tests
    {
        InMemoryAuctionRepository _auctions = new();
        CommandContext commandContext = CommandContext.CreateNew("test");
        Guid commandId = Guid.NewGuid();
        Guid correlationId = Guid.NewGuid();
        Guid buyerId = Guid.NewGuid();
        string paymentMethod = "test";
        EventBusMock eventBus = EventBusMock.Instance;

        [Fact]
        public async Task Should_be_completed_when_valid_events_are_published()
        {
            SetupServices(eventBus, out var paymentVerification, out var serviceProvider);

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            await auction.Buy(new UserId(buyerId), paymentMethod, paymentVerification.Object, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId = auction.TransactionId;
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, transactionId, out var ctx, out var sagaCoordinator);

            await sagaCoordinator.ProcessAsync(new BuyNowTXStarted
            {
                PaymentMethod = paymentMethod,
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
            SetupServices(eventBus, out var paymentVerification, out var serviceProvider);

            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            await auction.Buy(new UserId(buyerId), paymentMethod, paymentVerification.Object, Mock.Of<IAuctionUnlockScheduler>());
            var transactionId = auction.TransactionId;
            _auctions.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            SetupSagaContextAndCoordinator(serviceProvider, auction, correlationId, commandContext, transactionId, out var ctx, out var sagaCoordinator);

            await sagaCoordinator.ProcessAsync(new BuyNowTXStarted
            {
                PaymentMethod = paymentMethod,
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
                await auction.Buy(user2, "test", paymentVerification.Object, Mock.Of<IAuctionUnlockScheduler>());
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

        private void SetupServices(IEventBus eventBus, out Mock<IAuctionPaymentVerification> paymentVerification, out ServiceProvider serviceProvider)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCommon(new[] { Assembly.Load("Auctions.Application") });

            serviceCollection.AddChronicle(b => b.UseInMemoryPersistence());
            paymentVerification = new Mock<IAuctionPaymentVerification>();
            paymentVerification.Setup(f =>
f.Verification(It.IsAny<Auction>(), It.IsAny<UserId>(), It.IsAny<string>()))
.Returns(Task.FromResult(true));
            serviceCollection.AddTransient(typeof(Lazy<>), typeof(LazyInstance<>));
            serviceCollection.AddSingleton<IAuctionRepository>(_auctions);
            serviceCollection.AddTransient<IAuctionUnlockScheduler>(s => Mock.Of<IAuctionUnlockScheduler>());
            serviceCollection.AddScoped<EventOutboxMock>();
            serviceCollection.AddTransient<IEventOutbox, EventOutboxMock>(s => s.GetRequiredService<EventOutboxMock>());
            serviceCollection.AddTransient<IEventOutboxSavedItems, EventOutboxMock>(s => s.GetRequiredService<EventOutboxMock>());
            serviceCollection.AddTransient<IUnitOfWorkFactory>(s => UnitOfWorkFactoryMock.Instance.Object);
            serviceCollection.AddTransient<ISagaNotifications>(s => Mock.Of<ISagaNotifications>());
            serviceCollection.AddTransient<OptimisticConcurrencyHandler>();
            
            serviceCollection.AddSingleton<IEventBus>(eventBus);
            serviceCollection.AddTransient<IAppEventBuilder, TestAppEventBuilder>();
            serviceCollection.AddTransient<EventOutboxSender>();
            serviceCollection.AddTransient<IOutboxItemStore>(s => Mock.Of<IOutboxItemStore>());

            serviceCollection.AddLogging();
            //serviceCollection.AddTransient<EventBusHelper>(s => new EventBusHelper(eventBus, new TestAppEventBuilder()));
            serviceProvider = serviceCollection.BuildServiceProvider();

            CommonInstaller.InitAttributeStrategies("Auctions.Application");
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
