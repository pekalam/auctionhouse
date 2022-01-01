using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UserAddAuctionImage
{
    public class UserAddAuctionImageCommandHandler : CommandHandlerBase<UserAddAuctionImageCommand>
    {
        private readonly AuctionImageService _auctionImageService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusFacade _eventBusService;
        private readonly ILogger<UserAddAuctionImageCommandHandler> _logger;

        public UserAddAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, EventBusFacade eventBusService,
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
            //var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            //if (auction == null)
            //{
            //    throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
            //}

            //if (!auction.Owner.Equals(request.Command.SignedInUser))
            //{
            //    throw new CommandException(
            //        $"User {request.Command.SignedInUser} cannot modify auction ${auction.AggregateId}");
            //}

            //var file = File.ReadAllBytes(request.Command.TempPath);
            //File.Delete(request.Command.TempPath);

            //var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Command.Extension), file);

            //var newImg = _auctionImageService.AddAuctionImage(img);

            //auction.AddImage(newImg);

            //_auctionRepository.UpdateAuction(auction);
            //try
            //{
            //    _eventBusService.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
            //}
            //catch (Exception e)
            //{
            //    _logger.LogWarning(e, "Error while trying to publish events");
            //    _auctionImageService.RemoveAuctionImage(newImg);
            //    throw;
            //}
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<UserAddAuctionImageCommand> request,
            CancellationToken cancellationToken)
        {
            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            AddImage(request, cancellationToken);




            return Task.FromResult(response);
        }
    }
}