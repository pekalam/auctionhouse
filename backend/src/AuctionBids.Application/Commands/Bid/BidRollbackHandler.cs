﻿using Core.Command.Exceptions;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Auctions.Events;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Command.Bid
{
    public class BidRollbackHandler : ICommandRollbackHandler
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger _logger;

        public BidRollbackHandler(IImplProvider implProvider)
        {
            _logger = implProvider.Get<ILogger>();
            _auctionRepository = implProvider.Get<IAuctionRepository>();
        }

        public void Rollback(IAppEvent<Event> commandEvent)
        {

        }
    }
}
