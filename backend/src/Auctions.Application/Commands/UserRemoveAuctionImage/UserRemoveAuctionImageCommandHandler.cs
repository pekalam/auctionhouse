using System;
using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UserRemoveAuctionImage
{
    public class UserRemoveAuctionImageCommandHandler : CommandHandlerBase<UserRemoveAuctionImageCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger<UserRemoveAuctionImageCommandHandler> _logger;

        public UserRemoveAuctionImageCommandHandler(IAuctionRepository auctionRepository,
            ILogger<UserRemoveAuctionImageCommandHandler> logger, CommandHandlerBaseDependencies dependencies) 
            : base(ReadModelNotificationsMode.Immediate, dependencies)
        {
            _auctionRepository = auctionRepository;
            _logger = logger;
        }

        private void RemoveAuctionImage(AppCommand<UserRemoveAuctionImageCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
            }

            auction.RemoveImage(request.Command.ImgNum);


            _auctionRepository.UpdateAuction(auction);
            eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<UserRemoveAuctionImageCommand> request,
            IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            RemoveAuctionImage(request, eventOutbox, cancellationToken);

            return Task.FromResult(response);
        }
    }
}