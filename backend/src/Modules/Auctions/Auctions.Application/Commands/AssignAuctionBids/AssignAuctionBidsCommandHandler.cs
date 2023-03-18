using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Commands.Callbacks;
using Common.Application.Events;

namespace Auctions.Application.Commands.AssignAuctionBids
{
    public class AssignAuctionBidsCommandHandler : CommandHandlerBase<AssignAuctionBidsCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventOutbox _eventOutbox;

        public AssignAuctionBidsCommandHandler(CommandHandlerBaseDependencies dependencies,
            IAuctionRepository auctions, IUnitOfWorkFactory unitOfWorkFactory) : base(dependencies)
        {
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
            _eventOutbox = dependencies.EventOutbox;
            _auctions = auctions;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<AssignAuctionBidsCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);

            auction.AssignAuctionBids(new Domain.AuctionBidsId(request.Command.AuctionBidsId));

            using (var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.UpdateAuction(auction);
                await _eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                await _commandHandlerCallbacks.OnUowCommit(request);
                uow.Commit();
            }

            auction.MarkPendingEventsAsHandled();
            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
