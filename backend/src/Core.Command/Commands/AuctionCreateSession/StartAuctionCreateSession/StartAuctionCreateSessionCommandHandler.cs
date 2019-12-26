using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Domain.AuctionCreateSession;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.AuctionCreateSession.StartAuctionCreateSession
{
    public class StartAuctionCreateSessionCommandHandler : CommandHandlerBase<StartAuctionCreateSessionCommand>
    {
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly ILogger<StartAuctionCreateSessionCommandHandler> _logger;

        public StartAuctionCreateSessionCommandHandler(IAuctionCreateSessionService auctionCreateSessionService, ILogger<StartAuctionCreateSessionCommandHandler> logger) : base(logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(StartAuctionCreateSessionCommand request, CancellationToken cancellationToken)
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
                session = _auctionCreateSessionService.StartAndSaveNewSession();
            }

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED));
        }
    }
}