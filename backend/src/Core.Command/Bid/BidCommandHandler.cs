using System;
using System.Collections.Generic;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Bid;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Exceptions;
using Core.Common.Exceptions.Command;
using Microsoft.Extensions.Logging;

namespace Command.Bid
{
    public class BidCommandHandler : CommandHandlerBase<BidCommand>
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

            //weak solution to concurrency problem due to lack of transaction support in eventstore
            //TODO: external lock mechanism per auction / SQLServer as event database
            var check = _auctionRepository.FindAuction(request.AuctionId);
            var response = new RequestStatus(Status.PENDING);
            if (check.Version + 1 == auction.Version)
            {
                _auctionRepository.UpdateAuction(auction);
                _eventBusService.Publish(auction.PendingEvents, response.CorrelationId, request);
                auction.MarkPendingEventsAsHandled();
            }
            else
            {
                throw new CommandException("Invalid auction versions");
            }

            return Task.FromResult(response);
        }
    }
}