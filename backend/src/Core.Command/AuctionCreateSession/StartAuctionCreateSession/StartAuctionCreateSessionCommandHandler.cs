using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_StartAuctionCreateSession
{
    public class StartAuctionCreateSessionCommandHandler : CommandHandlerBase<StartAuctionCreateSessionCommand>
    {
        private readonly IEventSignalingService _eventSignalingService;
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly ILogger<StartAuctionCreateSessionCommandHandler> _logger;

        public StartAuctionCreateSessionCommandHandler(IEventSignalingService eventSignalingService, IAuctionCreateSessionService auctionCreateSessionService, ILogger<StartAuctionCreateSessionCommandHandler> logger) : base(logger)
        {
            _eventSignalingService = eventSignalingService;
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        protected override Task<CommandResponse> HandleCommand(StartAuctionCreateSessionCommand request, CancellationToken cancellationToken)
        {
            Common.Domain.AuctionCreateSession.AuctionCreateSession session;
            if (_auctionCreateSessionService.SessionExists())
            {
                session = _auctionCreateSessionService.GetExistingSession();
                session.ResetSession();
                _auctionCreateSessionService.SaveSession(session);
            }
            else
            {
                session = _auctionCreateSessionService.StartAndSaveNewSession();
            }
            _eventSignalingService
                .TrySendCompletionToUser("auctionCreateSessionStarted", request.CorrelationId, session.Creator);

            var response = new CommandResponse(request.CorrelationId, Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}