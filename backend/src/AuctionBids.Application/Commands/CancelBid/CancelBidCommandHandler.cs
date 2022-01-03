using AuctionBids.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.CancelBid
{
    public class CancelBidCommandHandler : CommandHandlerBase<CancelBidCommand>
    {
        private readonly IAuctionBidsRepository _auctionBids;

        public CancelBidCommandHandler(IAuctionBidsRepository auctionBids, ILogger<CancelBidCommandHandler> logger) : base(logger)
        {
            _auctionBids = auctionBids;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<CancelBidCommand> request, CancellationToken cancellationToken)
        {
            //TODO
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }

    }
}