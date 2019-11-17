using System;
using System.Collections.Generic;
using System.Text;
using Core.Common;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Core.Common.Interfaces;

namespace Core.Command.Bid
{
    public class BidRollbackHandler : ICommandRollbackHandler
    {
        private readonly IAuctionRepository _auctionRepository;

        public BidRollbackHandler(IImplProvider implProvider)
        {
            _auctionRepository = implProvider.Get<IAuctionRepository>();
        }

        public void Rollback(IAppEvent<Event> commandEvent)
        {
            var ev = (AuctionRaised)commandEvent.Event;
            var auction = _auctionRepository.FindAuction(ev.Bid.AuctionId);
            if (auction != null)
            {
                auction.RemoveBid(ev.Bid);
            }
            _auctionRepository.UpdateAuction(auction);
        }
    }
}
