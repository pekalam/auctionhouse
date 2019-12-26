using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.UserReplaceAuctionImage
{
    public class UserReplaceAuctionImageCommandHandler : CommandHandlerBase<UserReplaceAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<UserReplaceAuctionImageCommandHandler> _logger;

        public UserReplaceAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, EventBusService eventBusService,
            ILogger<UserReplaceAuctionImageCommandHandler> logger) : base(logger)
        {
            _auctionImageService = auctionImageService;
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        private void ReplaceAuctionImage(UserReplaceAuctionImageCommand request, CancellationToken cancellationToken, CorrelationId correlationId)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            var newImg = _auctionImageService.AddAuctionImage(request.Img);

            auction.ReplaceImage(newImg, request.ImgNum);

            _auctionRepository.UpdateAuction(auction);
            try
            {
                _eventBusService.Publish(auction.PendingEvents, correlationId, request);
            }
            catch (Exception e)
            {
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }
        }

        protected override Task<RequestStatus> HandleCommand(UserReplaceAuctionImageCommand request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.AuctionId);
            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            try
            {
                ReplaceAuctionImage(request, cancellationToken, response.CorrelationId);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}