using System;
using System.Collections.Generic;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Bid;
using Core.Command.Exceptions;
using Core.Command.Exceptions.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;

namespace Command.Bid
{
    public class BidCommandHandler : IRequestHandler<BidCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserIdentityService _userIdService;
        private readonly EventBusService _eventBusService;

        public BidCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdService,
            EventBusService eventBusService)
        {
            _auctionRepository = auctionRepository;
            _userIdService = userIdService;
            _eventBusService = eventBusService;
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(BidCommand),
                provider => new BidRollbackHandler(provider));
        }

        public virtual Task<Unit> Handle(BidCommand request, CancellationToken cancellationToken)
        {
            var signedInUser = _userIdService.GetSignedInUserIdentity();
            if (signedInUser == null)
            {
                throw new UserNotSignedInException("User not signed in");
            }

            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                throw new CommandException("Invalid auction id");
            }

            var bid = new Core.Common.Domain.Bids.Bid(auction.AggregateId, signedInUser, request.Price,
                DateTime.UtcNow);

            auction.Raise(bid);

            //weak solution to concurrency problem due to lack of transaction support in eventstore
            //TODO: external lock mechanism per auction / SQLServer as event database
            var check = _auctionRepository.FindAuction(request.AuctionId);
            if (check.Version + 1 == auction.Version)
            {
                _auctionRepository.UpdateAuction(auction);
                _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
                auction.MarkPendingEventsAsHandled();
            }
            else
            {
                throw new CommandException("Invalid auction versions");
            }
            

            return Task.FromResult(Unit.Value);
        }
    }
}