using System;
using System.Collections.Generic;
using System.Linq;
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
using Core.Common.Domain.Users;
using Core.Common.RequestStatusService;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.Bid
{
    public class BidCommandHandler : DecoratedCommandHandlerBase<BidCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserRepository _userRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<BidCommandHandler> _logger;
        private readonly IRequestStatusService _requestStatusService;

        public BidCommandHandler(IAuctionRepository auctionRepository, EventBusService eventBusService,
            ILogger<BidCommandHandler> logger, IRequestStatusService requestStatusService, IUserRepository userRepository) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            _requestStatusService = requestStatusService;
            _userRepository = userRepository;
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

            var user = _userRepository.FindUser(request.SignedInUser);
            if (user == null)
            {
                throw new CommandException($"Cannot find user {request.SignedInUser}");
            }
            
            auction.Raise(user, request.Price);
            var bid = auction.Bids.Last();

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.PENDING);
            _auctionRepository.UpdateAuction(auction);
            _userRepository.UpdateUser(user);
            var toSend = auction.PendingEvents.Concat(user.PendingEvents);
            _eventBusService.Publish(toSend, response.CorrelationId, request);
            _requestStatusService.TrySendNotificationToAll("AuctionPriceChanged", new Dictionary<string, object>()
            {
                {"winningBid", bid}
            });
            auction.MarkPendingEventsAsHandled();
            _logger.LogDebug("Bid {@bid} submited for an auction {@auction} by {@user}", bid, auction, request.SignedInUser);


            return Task.FromResult(response);
        }
    }
}