using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPaymentsEvents = UserPayments.DomainEvents.Events;

namespace Auctions.Application.Commands.BuyNow
{
    internal class MaxLockTimeElapsed
    {
    }

    internal class BuyNowSagaData
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }

    internal class BuyNowSaga : Saga<BuyNowSagaData>,
        ISagaStartAction<Events.V1.BuyNowTXStarted>,
        ISagaAction<UserPaymentsEvents.V1.BuyNowPaymentConfirmed>,
        ISagaAction<UserPaymentsEvents.V1.BuyNowPaymentFailed>
    {
        public const string AuctionContextParamName = "Auction";
        public const string TransactionContextParamName = "TransactionId";
        public const string CmdContextParamName = "CommandContext";

        private readonly Lazy<IAuctionRepository> _auctions;
        private readonly Lazy<IAuctionUnlockScheduler> _auctionUnlockScheduler;
        private readonly Lazy<IEventOutbox> _eventOutbox;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly Lazy<OptimisticConcurrencyHandler> _optimisticConcurrencyHandler;
        private readonly Lazy<EventOutboxSender> _eventOutboxSender;

        public BuyNowSaga(Lazy<IAuctionRepository> auctions, Lazy<IAuctionUnlockScheduler> auctionUnlockScheduler, Lazy<IEventOutbox> eventOutbox, 
            Lazy<ISagaNotifications> sagaNotifications, Lazy<OptimisticConcurrencyHandler> optimisticConcurrencyHandler, Lazy<EventOutboxSender> eventOutboxSender)
        {
            _auctions = auctions;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _eventOutbox = eventOutbox;
            _sagaNotifications = sagaNotifications;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
            _eventOutboxSender = eventOutboxSender;
        }

        public Task CompensateAsync(Events.V1.BuyNowTXStarted message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(Events.V1.BuyNowTXStarted message, ISagaContext context)
        {
            if (!context.TryGetMetadata(AuctionContextParamName, out var metadata))
            {
                throw new InvalidOperationException();
            }
            if (!context.TryGetMetadata(TransactionContextParamName, out var metadataTrans))
            {
                throw new InvalidOperationException();
            }

            var auction = (Auction)metadata.Value;
            Data = new BuyNowSagaData
            {
                AuctionId = auction.AggregateId,
                TransactionId = (Guid)metadataTrans.Value
            };

            return Task.CompletedTask;
        }

        private CommandContext GetCommandContextFromSagaContext(ISagaContext context)
        {
            if (!context.TryGetMetadata(CmdContextParamName, out var metadata))
            {
                throw new NullReferenceException();
            }

            return (CommandContext)metadata.Value;
        }

        public Task CompensateAsync(UserPaymentsEvents.V1.BuyNowPaymentConfirmed message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UserPaymentsEvents.V1.BuyNowPaymentConfirmed message, ISagaContext context)
        {
            var auction = _auctions.Value.FindAuction(Data.AuctionId);
            if (auction is null)
            {
                throw new NullReferenceException();
            }
            var commandContext = GetCommandContextFromSagaContext(context);

            OutboxItem[] outboxItems = null!;
            await _optimisticConcurrencyHandler.Value.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) auction = _auctions.Value.FindAuction(Data.AuctionId);
                auction.ConfirmBuy(Data.TransactionId, _auctionUnlockScheduler.Value);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.Value.UpdateAuction(auction);
                    outboxItems = await _eventOutbox.Value.SaveEvents(auction.PendingEvents, commandContext, ReadModelNotificationsMode.Saga);
                    await _sagaNotifications.Value.AddUnhandledEvents(commandContext.CorrelationId, auction.PendingEvents);
                    await _sagaNotifications.Value.MarkSagaAsCompleted(commandContext.CorrelationId); //TODO separate UOW for saga notifications
                    uow.Commit();
                }
            });
            await TrySendEvents(outboxItems);
            auction.MarkPendingEventsAsHandled();

            await CompleteAsync();
        }

        private async Task TrySendEvents(OutboxItem[] outboxItems)
        {
            try
            {
                await _eventOutboxSender.Value.SendEvents(outboxItems!);
            }
            catch (Exception)
            {
            }
        }

        public Task CompensateAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            var auction = _auctions.Value.FindAuction(Data.AuctionId);
            if (auction is null)
            {
                throw new NullReferenceException();
            }
            var commandContext = GetCommandContextFromSagaContext(context);

            OutboxItem[] outboxItems = null!;
            await _optimisticConcurrencyHandler.Value.Run(async (repeats, uowFactory) =>
            {
                if(repeats > 0) auction = _auctions.Value.FindAuction(Data.AuctionId);
                auction.CancelBuy(message.TransactionId, _auctionUnlockScheduler.Value);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.Value.UpdateAuction(auction);
                    outboxItems = await _eventOutbox.Value.SaveEvents(auction.PendingEvents, commandContext, ReadModelNotificationsMode.Saga);
                    await _sagaNotifications.Value.AddUnhandledEvents(commandContext.CorrelationId, auction.PendingEvents);
                    await _sagaNotifications.Value.MarkSagaAsFailed(commandContext.CorrelationId);
                    uow.Commit();
                }
            });
            await TrySendEvents(outboxItems);
            auction.MarkPendingEventsAsHandled();
            
            await RejectAsync();
        }
    }
}
