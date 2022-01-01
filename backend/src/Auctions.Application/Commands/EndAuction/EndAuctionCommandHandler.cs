using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.EndAuction
{
    public class EndAuctionCommandHandler : CommandHandlerBase<EndAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger<EndAuctionCommandHandler> _logger;

        public EndAuctionCommandHandler(IAuctionRepository auctionRepository, ILogger<EndAuctionCommandHandler> logger) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<EndAuctionCommand> request, CancellationToken cancellationToken)
        {


            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            return Task.FromResult(response);
        }
    }
}
