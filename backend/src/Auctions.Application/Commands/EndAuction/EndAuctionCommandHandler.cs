using System;
using Auctions.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.EndAuction
{
    public class EndAuctionCommandHandler : CommandHandlerBase<EndAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILogger<EndAuctionCommandHandler> _logger;

        public EndAuctionCommandHandler(IAuctionRepository auctionRepository, ILogger<EndAuctionCommandHandler> logger,
            Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Disabled, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _auctionRepository = auctionRepository;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<EndAuctionCommand> request,
            Lazy<EventBusFacade> eventBus, CancellationToken cancellationToken)
        {


            var response = RequestStatus.CreatePending(request.CommandContext);
            response.MarkAsCompleted();
            return Task.FromResult(response);
        }
    }
}
