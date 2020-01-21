using System;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;

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
            try
            {
                var cmd = (CreateAuctionCommand)commandEvent.CommandBase;
                var ev = (AuctionCreated)commandEvent.Event;
                _auctionRepository.RemoveAuction(ev.AuctionId);
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
