using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.UserRemoveAuctionImage
{
    public class UserRemoveAuctionImageCommandHandler : CommandHandlerBase<UserRemoveAuctionImageCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<UserRemoveAuctionImageCommandHandler> _logger;

        public UserRemoveAuctionImageCommandHandler(IAuctionRepository auctionRepository,
            EventBusService eventBusService,
            ILogger<UserRemoveAuctionImageCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        private void RemoveAuctionImage(UserRemoveAuctionImageCommand request, CancellationToken cancellationToken, CorrelationId correlationId)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            auction.RemoveImage(request.ImgNum);


            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, correlationId, request);
        }

        protected override Task<RequestStatus> HandleCommand(UserRemoveAuctionImageCommand request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.AuctionId);
            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            try
            {
                RemoveAuctionImage(request, cancellationToken, response.CorrelationId);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}