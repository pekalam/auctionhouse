using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.AuctionBids.Repositories;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.CancelBid
{
    public class CancelBidCommandHandler : CommandHandlerBase<CancelBidCommand>
    {
        private readonly IAuctionBidsRepository _auctionBids;
        private readonly IUserRepository _userRepository;
        private readonly EventBusService _eventBusService;

        public CancelBidCommandHandler(IUserRepository userRepository, IAuctionBidsRepository auctionBids, EventBusService eventBusService, ILogger<CancelBidCommandHandler> logger) : base(logger)
        {
            _userRepository = userRepository;
            _eventBusService = eventBusService;
            _auctionBids = auctionBids;
        }

        protected override Task<RequestStatus> HandleCommand(CancelBidCommand request, CancellationToken cancellationToken)
        {
            //TODO

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED));
        }

    }
}