using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Auctions.Application.Commands.BuyNow
{
    public class BuyNowCommandHandler : CommandHandlerBase<BuyNowCommand>
    {
        private readonly IAuctionPaymentVerification _auctionPaymentVerification;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusFacade _eventBusService;
        private readonly ILogger<BuyNowCommandHandler> _logger;

        public BuyNowCommandHandler(IAuctionRepository auctionRepository, EventBusFacade eventBusService, ILogger<BuyNowCommandHandler> logger, IAuctionPaymentVerification auctionPaymentVerification) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            _auctionPaymentVerification = auctionPaymentVerification;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BuyNowCommand> request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new ArgumentException($"Invalid auction id: {request.Command.AuctionId}");
            }

            _logger.LogDebug($"User {request.Command.SignedInUser} is buying auction {request.Command.AuctionId}");

            await auction.Buy(new UserId(request.Command.SignedInUser), "test", _auctionPaymentVerification);
            _eventBusService.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);

            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}