using Core.Common;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.Interfaces;

namespace Core.Command.CreateAuction
{
    public class CreateAuctionRollbackHandler : ICommandRollbackHandler
    {
        private readonly IAuctionRepository _auctionRepository;

        public CreateAuctionRollbackHandler(IImplProvider implProvider)
        {
            _auctionRepository = implProvider.Get<IAuctionRepository>();
        }

        public virtual void Rollback(IAppEvent<Event> commandEvent)
        {
            var cmd = (CreateAuctionCommand)commandEvent.Command;
            var ev = (AuctionCreated)commandEvent.Event;
            _auctionRepository.RemoveAuction(ev.AuctionId);
        }
    }
}
