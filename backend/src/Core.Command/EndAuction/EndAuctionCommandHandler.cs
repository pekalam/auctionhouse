using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.EndAuction
{
    public class EndAuctionCommandHandler : CommandHandlerBase<EndAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;
        private readonly ILogger<EndAuctionCommandHandler> _logger;

        public EndAuctionCommandHandler(IAuctionRepository auctionRepository, EventBusService eventBusService,
            ILogger<EndAuctionCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(EndAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            auction.EndAuction();
            _eventBusService.Publish(auction.PendingEvents, null, request);
            auction.MarkPendingEventsAsHandled();

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}
