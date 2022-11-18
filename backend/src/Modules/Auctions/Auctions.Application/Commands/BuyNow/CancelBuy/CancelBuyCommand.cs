using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;

namespace Auctions.Application.Commands.BuyNow.CancelBuy
{
    public class CancelBuyCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class CancelBuyCommandHandler : CommandHandlerBase<CancelBuyCommand>
    {
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;
        private readonly IAuctionRepository _auctions;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IAuctionUnlockScheduler _auctionUnlockScheduler;

        public CancelBuyCommandHandler(OptimisticConcurrencyHandler optimisticConcurrencyHandler, IAuctionRepository auctions, IAuctionUnlockScheduler auctionUnlockScheduler, ICommandHandlerCallbacks commandHandlerCallbacks,
            CommandHandlerBaseDependencies dependencies)
            : base(dependencies)
        {
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
            _auctions = auctions;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _commandHandlerCallbacks = commandHandlerCallbacks;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CancelBuyCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
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
                auction.CancelBuy(request.Command.TransactionId, _auctionUnlockScheduler);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.UpdateAuction(auction);
                    outboxItems = await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                    await _commandHandlerCallbacks.OnUowCommit(request);
                    uow.Commit();
                }
            });
            auction.MarkPendingEventsAsHandled();


            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
