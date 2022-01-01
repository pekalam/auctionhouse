using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.StartAuctionCreateSession
{
    public class StartAuctionCreateSessionCommandHandler : CommandHandlerBase<StartAuctionCreateSessionCommand>
    {
        private readonly IAuctionCreateSessionStore _auctionCreateSessionService;
        private readonly ILogger<StartAuctionCreateSessionCommandHandler> _logger;

        public StartAuctionCreateSessionCommandHandler(IAuctionCreateSessionStore auctionCreateSessionService, ILogger<StartAuctionCreateSessionCommandHandler> logger) : base(logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<StartAuctionCreateSessionCommand> request, CancellationToken cancellationToken)
        {
            AuctionCreateSession session;
            if (_auctionCreateSessionService.SessionExists())
            {
                _logger.LogDebug("Starting new AuctionCreateSession");
                session = _auctionCreateSessionService.GetExistingSession();
                session.ResetSession();
                _auctionCreateSessionService.SaveSession(session);
            }
            else
            {
                session = AuctionCreateSession.CreateSession(request.CommandContext.User);
                _auctionCreateSessionService.SaveSession(session);
            }
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}