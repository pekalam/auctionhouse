using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.DomainServices;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyNow
{
    public class BuyNowCommandHandlerTransactionDecorator : CommandHandlerBase<BuyNowCommand>
    {
        private BuyNowCommandHandler _buyNowCommandHandler;

        public BuyNowCommandHandlerTransactionDecorator(ILogger<BuyNowCommandHandlerTransactionDecorator> logger, BuyNowCommandHandler buyNowCommandHandler) : base(logger)
        {
            _buyNowCommandHandler = buyNowCommandHandler;
        }

        protected override Task<RequestStatus> HandleCommand(BuyNowCommand request, CancellationToken cancellationToken)
        {
            var transactionOpt = new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(15)
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOpt))
            {
                var status = _buyNowCommandHandler.Handle(request, cancellationToken);
                scope.Complete();

                return status;
            }
        }
    }

    public class BuyNowCommandHandler : DecoratedCommandHandlerBase<BuyNowCommand>
    {
        private IUserRepository _userRepository;
        private IAuctionRepository _auctionRepository;
        private EventBusService _eventBusService;
        private ILogger<BuyNowCommandHandler> _logger;

        public BuyNowCommandHandler(IUserRepository userRepository, IAuctionRepository auctionRepository, EventBusService eventBusService, ILogger<BuyNowCommandHandler> logger) : base(logger)
        {
            _userRepository = userRepository;
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(BuyNowCommand request, CancellationToken cancellationToken)
        {
            var user = _userRepository.FindUser(request.SignedInUser);

            if (user == null)
            {
                _logger.LogError("BuyNowCommandHandler cannot find user {@user}", request.SignedInUser);
                throw new CommandException($"Cannot find user: {user.UserIdentity}");
            }

            var auction = _auctionRepository.FindAuction(request.AuctionId);

            if (auction == null)
            {
                throw new CommandException($"Invalid auction id: {request.AuctionId}");
            }

            var buyNowService = new BuyNowService(_userRepository);

            var generatedEvents = buyNowService.BuyNow(auction, user);

            generatedEvents.AddRange(auction.PendingEvents);
            generatedEvents.AddRange(user.PendingEvents);

            _auctionRepository.UpdateAuction(auction);
            _userRepository.UpdateUser(user);

            _eventBusService.Publish(generatedEvents, request.CommandContext.CorrelationId, request);

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.PENDING));
        }
    }
}