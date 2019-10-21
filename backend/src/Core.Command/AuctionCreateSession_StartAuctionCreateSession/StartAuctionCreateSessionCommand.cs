using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.EventBus;
using Core.Common.EventSignalingService;
using Core.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession_StartAuctionCreateSession
{
    public class StartAuctionCreateSessionCommand : IRequest, ICommand
    {
        public CorrelationId CorrelationId { get; }

        public StartAuctionCreateSessionCommand(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }
    }

    public class StartAuctionCreateSessionCommandHandler : IRequestHandler<StartAuctionCreateSessionCommand>, ICommand
    {
        private readonly IEventSignalingService _eventSignalingService;
        private readonly IUserIdentityService _userIdentityService;
        private readonly IAppEventBuilder _appEventBuilder;
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly ILogger<StartAuctionCreateSessionCommandHandler> _logger;

        public StartAuctionCreateSessionCommandHandler(IEventSignalingService eventSignalingService, IUserIdentityService userIdentityService, IAppEventBuilder appEventBuilder, IAuctionCreateSessionService auctionCreateSessionService, ILogger<StartAuctionCreateSessionCommandHandler> logger)
        {
            _eventSignalingService = eventSignalingService;
            _userIdentityService = userIdentityService;
            _appEventBuilder = appEventBuilder;
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        public Task<Unit> Handle(StartAuctionCreateSessionCommand request, CancellationToken cancellationToken)
        {
            var userIdnIdentity = _userIdentityService.GetSignedInUserIdentity();
            if (userIdnIdentity == null)
            {
                throw new Exception("not signed in");
            }

            if (_auctionCreateSessionService.SessionExists())
            {
                var session = _auctionCreateSessionService.GetSessionForSignedInUser();
                session.ResetSession();
                _auctionCreateSessionService.SaveSessionForSignedInUser(session);
            }
            else
            {
                var session = userIdnIdentity.GetAuctionCreateSession();
                _auctionCreateSessionService.SaveSessionForSignedInUser(session);
            }
            _eventSignalingService
                .TrySendCompletionToUser("auctionCreateSessionStarted", request.CorrelationId, userIdnIdentity);
            return Task.FromResult(Unit.Value);
        }
    }
}