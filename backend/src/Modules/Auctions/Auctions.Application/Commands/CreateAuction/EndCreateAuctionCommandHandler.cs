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
        private readonly Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private readonly Lazy<IAuctionRepository> _auctions;
        private readonly Lazy<IAuctionImageRepository> _auctionImages;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventOutbox _eventOutbox;

        public EndCreateAuctionCommandHandler(CommandHandlerBaseDependencies dependencies,
            Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions,
            Lazy<IAuctionImageRepository> auctionImages, IUnitOfWorkFactory unitOfWorkFactory) :
            base(dependencies)
        {
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
            _eventOutbox = dependencies.EventOutbox;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            _auctionImages = auctionImages;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<EndCreateAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var createAuctionService = new CreateAuctionService(_auctionImages, _auctionEndScheduler, _auctions, request.Command.CreateAuctionServiceData);
            var auction = createAuctionService.EndCreate(new Domain.AuctionBidsId(request.Command.AuctionBidsId));

            //TODO rm:
            // auctionBidsAdded doesn't need to be projected in read model
            var eventsToSend = auction.PendingEvents.Where(e => e.EventName != "auctionBidsAdded");
            using (var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.Value.UpdateAuction(auction);
                await _eventOutbox.SaveEvents(eventsToSend, request.CommandContext);
                await _commandHandlerCallbacks.OnUowCommit(request);
                uow.Commit();
            }

            auction.MarkPendingEventsAsHandled();
            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
