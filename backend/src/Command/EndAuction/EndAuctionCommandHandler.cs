using System.Threading;
using System.Threading.Tasks;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using MediatR;

namespace Core.Command.EndAuction
{
    public class EndAuctionCommandHandler : IRequestHandler<EndAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusService _eventBusService;

        public EndAuctionCommandHandler(IAuctionRepository auctionRepository, EventBusService eventBusService)
        {
            _auctionRepository = auctionRepository;
            _eventBusService = eventBusService;
        }

        public Task<Unit> Handle(EndAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            auction.EndAuction();
            _eventBusService.Publish(auction.PendingEvents, null, request);
            auction.MarkPendingEventsAsHandled();

            return Task.FromResult(Unit.Value);
        }
    }
}
