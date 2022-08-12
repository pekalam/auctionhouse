using System;
using AuctionBids.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.CancelBid
{
    public class CancelBidCommandHandler : CommandHandlerBase<CancelBidCommand>
    {
        private readonly IAuctionBidsRepository _auctionBids;

        public CancelBidCommandHandler(IAuctionBidsRepository auctionBids, ILogger<CancelBidCommandHandler> logger, CommandHandlerBaseDependencies dependencies) 
            : base(ReadModelNotificationsMode.Immediate, dependencies)
        {
            _auctionBids = auctionBids;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<CancelBidCommand> request,
            IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            //TODO
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }

    }
}