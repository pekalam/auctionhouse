using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.AuctionCreateSession.StartAuctionCreateSession
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
            Common.Domain.AuctionCreateSession.AuctionCreateSession session;
            if (_auctionCreateSessionService.SessionExists())
            {
                _logger.LogDebug("Starting new AuctionCreateSession");
                session = _auctionCreateSessionService.GetExistingSession();
                session.ResetSession();
                _auctionCreateSessionService.SaveSession(session);
            }
            else
            {
                session = Common.Domain.AuctionCreateSession.AuctionCreateSession.CreateSession(request.CommandContext.User);
                _auctionCreateSessionService.SaveSession(session);
            }
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}