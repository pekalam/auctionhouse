using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UserRemoveAuctionImage
{
    public class UserRemoveAuctionImageCommandHandler : CommandHandlerBase<UserRemoveAuctionImageCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusFacade _eventBusService;
        private readonly ILogger<UserRemoveAuctionImageCommandHandler> _logger;

        public UserRemoveAuctionImageCommandHandler(IAuctionRepository auctionRepository,
            EventBusFacade eventBusService,
            ILogger<UserRemoveAuctionImageCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        private void RemoveAuctionImage(AppCommand<UserRemoveAuctionImageCommand> request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.Command.AuctionId}");
            }

            auction.RemoveImage(request.Command.ImgNum);


            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<UserRemoveAuctionImageCommand> request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.Command.AuctionId);
            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            try
            {
                RemoveAuctionImage(request, cancellationToken);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.Command.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}