using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Bid;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.AuctionBids.Repositories;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.RequestStatusSender;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.Bid
{
    public class BidCommandHandler : DecoratedCommandHandlerBase<BidCommand>
    {
        private readonly IAuctionBidsRepository _auctionBids;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<BidCommandHandler> _logger;

        public BidCommandHandler(EventBusService eventBusService, ILogger<BidCommandHandler> logger) : base(logger)
        {
            _eventBusService = eventBusService;
            _logger = logger;
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(BidCommand),
                provider => new BidRollbackHandler(provider));
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BidCommand> request, CancellationToken cancellationToken)
        {
            var auctionBids = _auctionBids.WithAuctionId(new Common.Domain.AuctionBids.AuctionId(request.Command.AuctionId));
            if(auctionBids is null)
            {
                throw new CommandException($"AuctionBids {request.Command.AuctionId} could not be found");
            }


            var bid = auctionBids.TryRaise(new Common.Domain.AuctionBids.UserId(request.Command.SignedInUser), request.Command.Price);

            _eventBusService.Publish(auctionBids.PendingEvents, request.CommandContext, Common.EventBus.ReadModelNotificationsMode.Immediate);
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