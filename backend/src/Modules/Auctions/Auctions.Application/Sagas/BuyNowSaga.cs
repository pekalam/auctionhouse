﻿using Auctions.Application.Commands.CancelBuy;
using Auctions.Application.Commands.ConfirmBuy;
using Auctions.Domain;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application.Commands;
using Common.Application.Mediator;
using UserPaymentsEvents = UserPayments.DomainEvents.Events;

namespace Auctions.Application.Sagas
{
    public class BuyNowSagaData
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class BuyNowSaga : Saga<BuyNowSagaData>,
        ISagaStartAction<Events.V1.AuctionBought>,
        ISagaAction<UserPaymentsEvents.V1.BuyNowPaymentConfirmed>,
        ISagaAction<UserPaymentsEvents.V1.BuyNowPaymentFailed>
    {
        public const string AuctionContextParamName = "Auction";
        public const string TransactionContextParamName = "TransactionId";
        public const string CmdContextParamName = "CommandContext";

        private readonly Lazy<CommandQueryMediator> _mediator;

        public BuyNowSaga(Lazy<CommandQueryMediator> mediator)
        {
            _mediator = mediator;
        }

        public Task CompensateAsync(Events.V1.AuctionBought message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(Events.V1.AuctionBought message, ISagaContext context)
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
            var cmd = new ConfirmBuyCommand
            {
                AuctionId = Data.AuctionId,
                TransactionId = Data.TransactionId,
            };
            await _mediator.Value.Send(cmd, GetCommandContextFromSagaContext(context));

            await CompleteAsync();
        }

        public Task CompensateAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public async Task HandleAsync(UserPaymentsEvents.V1.BuyNowPaymentFailed message, ISagaContext context)
        {
            var cmd = new CancelBuyCommand
            {
                AuctionId = Data.AuctionId,
                TransactionId = message.TransactionId,
            };
            await _mediator.Value.Send(cmd, GetCommandContextFromSagaContext(context));

            await RejectAsync();
        }
    }
}
