using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Commands.Callbacks;
using Common.Application.Events;

namespace Auctions.Application.Commands.CreateAuction
{
    public class EndCreateAuctionCommandHandler : CommandHandlerBase<EndCreateAuctionCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventOutbox _eventOutbox;

        public EndCreateAuctionCommandHandler(CommandHandlerBaseDependencies dependencies,
            IAuctionRepository auctions, IUnitOfWorkFactory unitOfWorkFactory) : base(dependencies)
        {
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
            _eventOutbox = dependencies.EventOutbox;
            _auctions = auctions;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<EndCreateAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);

            auction.AddAuctionBids(new Domain.AuctionBidsId(request.Command.AuctionBidsId));

            //TODO rm:
            // auctionBidsAdded doesn't need to be projected in read model
            var eventsToSend = auction.PendingEvents.Where(e => e.EventName != "auctionBidsAdded");
            using (var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.UpdateAuction(auction);
                await _eventOutbox.SaveEvents(eventsToSend, request.CommandContext);
                await _commandHandlerCallbacks.OnUowCommit(request);
                uow.Commit();
            }

            auction.MarkPendingEventsAsHandled();
            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
