using Common.Application;
using Core.Common.Domain;

namespace Auctions.Application.Commands.CreateAuction
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
                //var cmd = (CreateAuctionCommand)commandEvent.CommandBase;
                //var ev = (AuctionCreated)commandEvent.Event;
                //_auctionRepository.RemoveAuction(ev.AuctionId);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
