using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Command.Bid;
using Core.DomainFramework;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.Bid
{
    public class RaiseBidCommandHandler : CommandHandlerBase<RaiseBidCommand>
    {
        private readonly IAuctionBidsRepository _allAuctionBids;
        private readonly ILogger<RaiseBidCommandHandler> _logger;
        private readonly IUnitOfWorkFactory _uowFactory;

        public RaiseBidCommandHandler(ILogger<RaiseBidCommandHandler> logger, CommandHandlerBaseDependencies dependencies, IUnitOfWorkFactory uowFactory, IAuctionBidsRepository auctionBids)
            : base(ReadModelNotificationsMode.Immediate, dependencies)
        {
            _logger = logger;
            _uowFactory = uowFactory;
            _allAuctionBids = auctionBids;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<RaiseBidCommand> request,
            IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auctionBids = _allAuctionBids.WithAuctionId(new AuctionId(request.Command.AuctionId));
            if (auctionBids is null)
            {
                throw new ArgumentException($"AuctionBids {request.Command.AuctionId} could not be found");
            }

            AuctionBids.Domain.Bid bid;
            try
            {
                bid = auctionBids.TryRaise(new UserId(request.Command.SignedInUser), request.Command.Price);
            }
            catch (DomainException)
            {
                return RequestStatus.CreateFailed(request.CommandContext);
            }

            _logger.LogDebug("Bid {@bid} submited for an auction {@auction} by {@user}", bid, request.Command.AuctionId, request.Command.SignedInUser);
            using (var uow = _uowFactory.Begin())
            {
                _allAuctionBids.Update(auctionBids);
                if (bid.Accepted) //accepted bid should be visible immediatedly
                {
                    await eventOutbox.SaveEvents(auctionBids.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
                }
                else //bid that is not accepted dont't need that
                {
                    await eventOutbox.SaveEvents(auctionBids.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                }
                auctionBids.MarkPendingEventsAsHandled();
                uow.Commit();
            }

            if (!bid.Accepted)
            {
                return RequestStatus.CreateFailed(request.CommandContext);
            }
            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}