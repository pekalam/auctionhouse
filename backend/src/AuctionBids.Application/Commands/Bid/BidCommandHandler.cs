using System;
using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Command.Bid;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.Bid
{
    public class BidCommandHandler : CommandHandlerBase<BidCommand>
    {
        private readonly IAuctionBidsRepository _auctionBids;
        private readonly ILogger<BidCommandHandler> _logger;

        public BidCommandHandler(ILogger<BidCommandHandler> logger, Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Disabled, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _logger = logger;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BidCommand> request,
            Lazy<EventBusFacade> eventBus, CancellationToken cancellationToken)
        {
            var auctionBids = _auctionBids.WithAuctionId(new AuctionId(request.Command.AuctionId));
            if (auctionBids is null)
            {
                throw new ArgumentException($"AuctionBids {request.Command.AuctionId} could not be found");
            }


            var bid = auctionBids.TryRaise(new UserId(request.Command.SignedInUser), request.Command.Price);

            //_eventBusService.Publish(auctionBids.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
            //if (bid.Accepted)
            //{
            //    _requestStatusService.TrySendNotificationToAll("AuctionPriceChanged", new Dictionary<string, object>()
            //    {
            //        {"winningBid", bid}
            //    });
            //}
            _logger.LogDebug("Bid {@bid} submited for an auction {@auction} by {@user}", bid, request.Command.AuctionId, request.Command.SignedInUser);


            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}