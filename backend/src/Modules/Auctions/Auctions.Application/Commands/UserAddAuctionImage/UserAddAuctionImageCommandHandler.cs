using Auctions.Domain;
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
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<UserAddAuctionImageCommandHandler> _logger;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public UserAddAuctionImageCommandHandler(AuctionImageService auctionImageService,
            IAuctionRepository auctionRepository, ILogger<UserAddAuctionImageCommandHandler> logger,
            CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(dependencies)
        {
            _auctionImageService = auctionImageService;
            _auctions = auctionRepository;
            _logger = logger;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<UserAddAuctionImageCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
            }

            if (!auction.Owner.Value.Equals(request.Command.SignedInUser))
            {
                throw new InvalidOperationException(
                    $"User {request.Command.SignedInUser} cannot modify auction ${auction.AggregateId}");
            }

            var file = File.ReadAllBytes(request.Command.TempPath);
            File.Delete(request.Command.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Command.Extension), file);
            var newImg = _auctionImageService.AddAuctionImage(img);

            try
            {
                await _optimisticConcurrencyHandler.Run(
                    async (repeat, uowFactory) =>
                    {
                        if (repeat > 0)
                        {
                            auction = _auctions.FindAuction(request.Command.AuctionId);
                        }
                        auction.AddImage(newImg);

                        using (var uow = uowFactory.Begin())
                        {
                            _auctions.UpdateAuction(auction);
                            await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                            uow.Commit();
                        }
                    });
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error while trying to persist auction changes - removing added images");
                _auctionImageService.RemoveAuctionImage(newImg);
                throw;
            }

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}