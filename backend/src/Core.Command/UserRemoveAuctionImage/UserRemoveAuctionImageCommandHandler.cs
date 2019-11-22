using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Exceptions.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.RemoveAuctionImage.Core.Command.RemoveAuctionImage
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

        private void RemoveAuctionImage(UserRemoveAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            auction.RemoveImage(request.ImgNum);


            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
        }

        protected override Task<CommandResponse> HandleCommand(UserRemoveAuctionImageCommand request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.AuctionId);

            try
            {
                RemoveAuctionImage(request, cancellationToken);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.AuctionId);
            }


            var response = new CommandResponse(request.CorrelationId, Status.PENDING);
            return Task.FromResult(response);
        }
    }
}