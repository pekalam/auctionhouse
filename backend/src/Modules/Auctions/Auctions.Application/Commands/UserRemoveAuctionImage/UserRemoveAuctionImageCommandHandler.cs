using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UserRemoveAuctionImage
{
    public class UserRemoveAuctionImageCommandHandler : CommandHandlerBase<UserRemoveAuctionImageCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<UserRemoveAuctionImageCommandHandler> _logger;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public UserRemoveAuctionImageCommandHandler(IAuctionRepository auctionRepository, ILogger<UserRemoveAuctionImageCommandHandler> logger, 
            CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(ReadModelNotificationsMode.Immediate, dependencies)
        {
            _auctions = auctionRepository;
            _logger = logger;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<UserRemoveAuctionImageCommand> request, IEventOutbox eventOutbox,
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

            await _optimisticConcurrencyHandler.Run(
                async (repeat, uowFactory) =>
                {
                    if (repeat > 0) auction = _auctions.FindAuction(request.Command.AuctionId);
                    auction.RemoveImage(request.Command.ImgNum);

                    using (var uow = uowFactory.Begin())
                    {
                        _auctions.UpdateAuction(auction);
                        await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
                        uow.Commit();
                    }
                });



            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}