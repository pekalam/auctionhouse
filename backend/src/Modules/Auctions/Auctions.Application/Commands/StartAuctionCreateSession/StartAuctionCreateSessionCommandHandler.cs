using System;
using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.StartAuctionCreateSession
{
    public class StartAuctionCreateSessionCommandHandler : CommandHandlerBase<StartAuctionCreateSessionCommand>
    {
        private readonly IAuctionCreateSessionStore _auctionCreateSessionStore;
        private readonly ILogger<StartAuctionCreateSessionCommandHandler> _logger;

        public StartAuctionCreateSessionCommandHandler(IAuctionCreateSessionStore auctionCreateSessionStore, ILogger<StartAuctionCreateSessionCommandHandler> logger,
            CommandHandlerBaseDependencies dependencies) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _auctionCreateSessionStore = auctionCreateSessionStore;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(
            AppCommand<StartAuctionCreateSessionCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            AuctionCreateSession session;
            if (_auctionCreateSessionStore.SessionExists())
            {
                _logger.LogDebug("Session already exists - starting new AuctionCreateSession");
                session = _auctionCreateSessionStore.GetExistingSession();
                session.ResetSession();
                _auctionCreateSessionStore.SaveSession(session);
            }
            else
            {
                session = AuctionCreateSession.CreateSession(request.CommandContext.User);
                _auctionCreateSessionStore.SaveSession(session);
            }
            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}