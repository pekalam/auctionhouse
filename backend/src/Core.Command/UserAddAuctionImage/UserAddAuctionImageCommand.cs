using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.DomainServices;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AddOrReplaceAuctionImage
{
    public class UserAddAuctionImageCommand : IRequest, ICommand
    {
        public Guid AuctionId { get; }
        public AuctionImageRepresentation Img { get; }
        public CorrelationId CorrelationId { get; }

        public UserAddAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, CorrelationId correlationId)
        {
            AuctionId = auctionId;
            Img = img;
            CorrelationId = correlationId;
        }
    }

    public class UserAddAuctionImageCommandHandler : IRequestHandler<UserAddAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<UserAddAuctionImageCommandHandler> _logger;
        private readonly IUserIdentityService _userIdentityService;

        public UserAddAuctionImageCommandHandler(AuctionImageService auctionImageService, IAuctionRepository auctionRepository, EventBusService eventBusService, ILogger<UserAddAuctionImageCommandHandler> logger, IUserIdentityService userIdentityService)
        {
            _auctionImageService = auctionImageService;
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
            _userIdentityService = userIdentityService;
        }

        public Task<Unit> Handle(UserAddAuctionImageCommand request, CancellationToken cancellationToken)
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
            return Task.FromResult(Unit.Value);
        }
    }
}
