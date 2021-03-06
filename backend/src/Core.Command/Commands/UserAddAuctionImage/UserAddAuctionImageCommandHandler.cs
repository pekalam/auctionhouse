﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.UserAddAuctionImage
{
    public class UserAddAuctionImageCommandHandler : CommandHandlerBase<UserAddAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<UserAddAuctionImageCommandHandler> _logger;

        public UserAddAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, EventBusService eventBusService,
            ILogger<UserAddAuctionImageCommandHandler> logger)
            : base(logger)
        {
            _auctionImageService = auctionImageService;
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        private void AddImage(UserAddAuctionImageCommand request, CancellationToken cancellationToken, CorrelationId correlationId)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.AuctionId}");
            }

            if (!auction.Owner.UserId.Equals(request.SignedInUser.UserId))
            {
                throw new CommandException(
                    $"User {request.SignedInUser.UserId} cannot modify auction ${auction.AggregateId}");
            }

            var file = File.ReadAllBytes(request.TempPath);
            File.Delete(request.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Extension), file);

            var newImg = _auctionImageService.AddAuctionImage(img);

            auction.AddImage(newImg);

            _auctionRepository.UpdateAuction(auction);
            try
            {
                _eventBusService.Publish(auction.PendingEvents, correlationId, request);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error while trying to publish events");
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