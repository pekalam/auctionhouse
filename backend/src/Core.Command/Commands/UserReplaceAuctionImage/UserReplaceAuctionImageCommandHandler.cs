using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.Auction.Services;
using Core.Common.Domain.Auctions;
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

        private void ReplaceAuctionImage(AppCommand<UserReplaceAuctionImageCommand> request)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new CommandException($"Cannot find auction {request.Command.AuctionId}");
            }

            var file = File.ReadAllBytes(request.Command.TempPath);
            File.Delete(request.Command.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Command.Extension), file);

            var newImg = _auctionImageService.AddAuctionImage(img);

            auction.ReplaceImage(newImg, request.Command.ImgNum);

            _auctionRepository.UpdateAuction(auction);
            try
            {
                _eventBusService.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
            }
            catch (Exception)
            {
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<UserReplaceAuctionImageCommand> request,
            CancellationToken cancellationToken)
        {
            AuctionLock.Lock(request.Command.AuctionId);
            var response = RequestStatus.CreatePending(request.CommandContext);
            try
            {
                ReplaceAuctionImage(request);
            }
            finally
            {
                AuctionLock.ReleaseLock(request.Command.AuctionId);
            }


            return Task.FromResult(response);
        }
    }
}