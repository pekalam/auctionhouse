using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
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

        protected override Task<CommandResponse> HandleCommand(UserAddAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var signedInUserIdentity = _userIdentityService.GetSignedInUserIdentity();
            if (signedInUserIdentity == null)
            {
                throw new CommandException("");
            }

            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            if (!auction.Owner.UserId.Equals(signedInUserIdentity.UserId))
            {
                throw new CommandException($"User {signedInUserIdentity.UserId} cannot modify auction ${auction.AggregateId}");
            }


            var newImg = _auctionImageService.AddAuctionImage(request.Img);

            auction.AddImage(newImg);

            try
            {
                _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while trying to publish events {e.Message}");
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }
            _auctionRepository.UpdateAuction(auction);

            var response = new CommandResponse(Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}