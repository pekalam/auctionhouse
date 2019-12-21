using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Bid;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.Bid
{
    public class BidCommandHandler : DecoratedCommandHandlerBase<BidCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<BidCommandHandler> _logger;

        public BidCommandHandler(IAuctionRepository auctionRepository,
            EventBusService eventBusService, ILogger<BidCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(BidCommand),
                provider => new BidRollbackHandler(provider));
        }

        protected override Task<RequestStatus> HandleCommand(BidCommand request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                throw new CommandException("Invalid auction id");
            }

            var bid = new Core.Common.Domain.Bids.Bid(auction.AggregateId, request.SignedInUser, request.Price,
                DateTime.UtcNow);

            auction.Raise(bid);

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, response.CorrelationId, request);
            auction.MarkPendingEventsAsHandled();

            return Task.FromResult(response);
        }
    }
}