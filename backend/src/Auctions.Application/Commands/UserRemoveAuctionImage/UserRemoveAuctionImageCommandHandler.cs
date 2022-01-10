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
            ILogger<UserRemoveAuctionImageCommandHandler> logger, Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Immediate, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _auctionRepository = auctionRepository;
            _logger = logger;
        }

        private void RemoveAuctionImage(AppCommand<UserRemoveAuctionImageCommand> request, EventBusFacade eventBus, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
            }

            auction.RemoveImage(request.Command.ImgNum);


            _auctionRepository.UpdateAuction(auction);
            eventBus.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<UserRemoveAuctionImageCommand> request,
            Lazy<EventBusFacade> eventBus,
            CancellationToken cancellationToken)
        {
            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            RemoveAuctionImage(request, eventBus.Value, cancellationToken);




            return Task.FromResult(response);
        }
    }
}