﻿using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Commands.Callbacks;
using Common.Application.Events;

namespace Auctions.Application.Commands.ConfirmBuy
{
    public class ConfirmBuyCommandHandler : CommandHandlerBase<ConfirmBuyCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IAuctionBuyCancellationScheduler _auctionBuyCancellationScheduler;

        public ConfirmBuyCommandHandler(CommandHandlerBaseDependencies dependencies, IAuctionRepository auctions, OptimisticConcurrencyHandler optimisticConcurrencyHandler, ICommandHandlerCallbacks commandHandlerCallbacks, IAuctionBuyCancellationScheduler auctionBuyCancellationScheduler) : base(dependencies)
        {
            _auctions = auctions;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
            _auctionBuyCancellationScheduler = auctionBuyCancellationScheduler;
            _commandHandlerCallbacks = commandHandlerCallbacks;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<ConfirmBuyCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);
            if (auction is null)
            {
                throw new NullReferenceException();
            }

            OutboxItem[] outboxItems = null!;
            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) auction = _auctions.FindAuction(request.Command.AuctionId);
                auction.ConfirmBuy(request.Command.TransactionId, _auctionBuyCancellationScheduler);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.UpdateAuction(auction);
                    outboxItems = await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                    await _commandHandlerCallbacks.OnUowCommit(request); //TODO separate UOW for saga notifications
                    uow.Commit();
                }
            });
            auction.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
