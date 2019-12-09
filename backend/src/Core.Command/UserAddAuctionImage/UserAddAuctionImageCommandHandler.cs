using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.RemoveAuctionImage.Core.Command.RemoveAuctionImage;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventBus;
using Core.Common.Exceptions.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.AddOrReplaceAuctionImage
{
    public class UserAddAuctionImageCommandHandler : CommandHandlerBase<UserAddAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<UserAddAuctionImageCommandHandler> _logger;
        private readonly IUserIdentityService _userIdentityService;

        public UserAddAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, EventBusService eventBusService,
            ILogger<UserAddAuctionImageCommandHandler> logger, IUserIdentityService userIdentityService)
            : base(logger)
        {
            _auctionImageService = auctionImageService;
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            _userIdentityService = userIdentityService;
        }

        private void AddImage(UserAddAuctionImageCommand request, CancellationToken cancellationToken, CorrelationId correlationId)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            if (!auction.Owner.UserId.Equals(request.SignedInUser.UserId))
            {
                throw new CommandException(
                    $"User {request.SignedInUser.UserId} cannot modify auction ${auction.AggregateId}");
            }


            var newImg = _auctionImageService.AddAuctionImage(request.Img);

            auction.AddImage(newImg);

            _auctionRepository.UpdateAuction(auction);
            try
            {
                _eventBusService.Publish(auction.PendingEvents, correlationId, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while trying to publish events {e.Message}");
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }
        }

        protected override Task<RequestStatus> HandleCommand(UserAddAuctionImageCommand request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.AuctionId);
            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            try
            {
                AddImage(request, cancellationToken, response.CorrelationId);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}