using System;
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
using Core.Common.Command;
using Core.Common.Domain.Auction.Services;
using Core.Common.Domain.Auctions;
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

        private void AddImage(AppCommand<UserAddAuctionImageCommand> request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.Command.AuctionId}");
            }

            if (!auction.Owner.Equals(request.Command.SignedInUser))
            {
                throw new CommandException(
                    $"User {request.Command.SignedInUser} cannot modify auction ${auction.AggregateId}");
            }

            var file = File.ReadAllBytes(request.Command.TempPath);
            File.Delete(request.Command.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Command.Extension), file);

            var newImg = _auctionImageService.AddAuctionImage(img);

            auction.AddImage(newImg);

            _auctionRepository.UpdateAuction(auction);
            try
            {
                _eventBusService.Publish(auction.PendingEvents, request.CommandContext);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error while trying to publish events");
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<UserAddAuctionImageCommand> request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.Command.AuctionId);
            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            try
            {
                AddImage(request, cancellationToken);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.Command.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}