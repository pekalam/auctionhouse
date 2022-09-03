using System;
using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.EndAuction
{
    public class EndAuctionCommandHandler : CommandHandlerBase<EndAuctionCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<EndAuctionCommandHandler> _logger;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public EndAuctionCommandHandler(IAuctionRepository auctionRepository, ILogger<EndAuctionCommandHandler> logger,
                CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _auctions = auctionRepository;
            _logger = logger;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<EndAuctionCommand> request,
            IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);

            if (auction is null)
            {
                _logger.LogWarning("Could not find auction with id {@auctionId}", request.Command.AuctionId);
                throw new InvalidCommandException("Could not find auction with id " + request.Command.AuctionId);
            }

            _logger.LogInformation("Ending auction {@auctionId}", request.Command.AuctionId);

            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) => {
                if(repeats > 0) auction = _auctions.FindAuction(request.Command.AuctionId);

                using(var uow = uowFactory.Begin())
                {
                    auction.EndAuction();
                    _auctions.UpdateAuction(auction);
                    await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
                    uow.Commit();
                }
            });

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
