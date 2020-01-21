using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.CancelBid
{
    public class CancelBidCommandHandler : CommandHandlerBase<CancelBidCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserRepository _userRepository;
        private readonly EventBusService _eventBusService;

        public CancelBidCommandHandler(IAuctionRepository auctionRepository, IUserRepository userRepository, EventBusService eventBusService, ILogger<CancelBidCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _userRepository = userRepository;
            _eventBusService = eventBusService;
        }

        protected override Task<RequestStatus> HandleCommand(CancelBidCommand request, CancellationToken cancellationToken)
        {
            var auction = FindAuction(request);
            var bid = FindBid(request, auction);
            var user = FindUser(request);

            auction.CancelBid(user, bid);


            var transactionOpt = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(10)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOpt))
            {
                _auctionRepository.UpdateAuction(auction);
                _userRepository.UpdateUser(user);
                scope.Complete();
            }

            
            _eventBusService.Publish(auction.PendingEvents.Concat(user.PendingEvents), request.CommandContext.CorrelationId, request);
            auction.MarkPendingEventsAsHandled();
            user.MarkPendingEventsAsHandled();

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED));
        }

        private User FindUser(CancelBidCommand request)
        {
            var user = _userRepository.FindUser(request.SignedInUser);

            if (user == null)
            {
                throw new CommandException($"Cannot find user {request.SignedInUser}");
            }

            return user;
        }

        private static Common.Domain.Bids.Bid FindBid(CancelBidCommand request, Auction auction)
        {
            var bid = auction.Bids.FirstOrDefault(b => b.BidId == request.BidId);

            if (bid == null)
            {
                throw new CommandException($"Cannot find bid {request.BidId}");
            }

            return bid;
        }

        private Auction FindAuction(CancelBidCommand request)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);

            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            return auction;
        }
    }
}