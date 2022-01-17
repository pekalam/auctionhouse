using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application.Commands;
using Common.Application.Events;
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
        private readonly Lazy<EventBusHelper> _eventBusHelper;


        public BuyNowSaga(Lazy<IAuctionRepository> auctions, Lazy<IAuctionUnlockScheduler> auctionUnlockScheduler, Lazy<EventBusHelper> eventBusHelper)
        {
            _auctions = auctions;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _eventBusHelper = eventBusHelper;
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
            if(!context.TryGetMetadata(CmdContextParamName, out var metadata))
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
            if(auction is null)
            {
                throw new NullReferenceException();
            }
            auction.ConfirmBuy(Data.TransactionId, _auctionUnlockScheduler.Value);
            _auctions.Value.UpdateAuction(auction); //TODO transaction
            _eventBusHelper.Value.Publish(auction.PendingEvents, GetCommandContextFromSagaContext(context), ReadModelNotificationsMode.Saga);
            auction.MarkPendingEventsAsHandled();

            await CompleteAsync();
        }


        public Task CompensateAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            var auction = _auctions.Value.FindAuction(Data.AuctionId);
            if (auction is null)
            {
                throw new NullReferenceException();
            }

            auction.CancelBuy(message.TransactionId, _auctionUnlockScheduler.Value);
            _eventBusHelper.Value.Publish(auction.PendingEvents, GetCommandContextFromSagaContext(context), ReadModelNotificationsMode.Saga);
            auction.MarkPendingEventsAsHandled();

            return RejectAsync();
        }
    }
}
