using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UserReplaceAuctionImage
{
    public class UserReplaceAuctionImageCommandHandler : CommandHandlerBase<UserReplaceAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusFacade _eventBusService;
        private readonly ILogger<UserReplaceAuctionImageCommandHandler> _logger;

        public UserReplaceAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, EventBusFacade eventBusService,
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
                throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
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
            var response = RequestStatus.CreatePending(request.CommandContext);
            ReplaceAuctionImage(request);



            return Task.FromResult(response);
        }
    }
}