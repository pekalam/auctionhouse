using System;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.BuyNow
{
    public class BuyNowCommandHandler : CommandHandlerBase<BuyNowCommand>
    {
        private readonly ILogger<BuyNowCommandHandler> _logger;
        private readonly IAuctionUnlockScheduler _auctionUnlockScheduler;
        private readonly IAuctionPaymentVerification _auctionPaymentVerification;
        private readonly IAuctionRepository _auctionRepository;
        private readonly ISagaCoordinator _sagaCoordinator;

        public BuyNowCommandHandler(IAuctionRepository auctionRepository, ILogger<BuyNowCommandHandler> logger, 
            IAuctionPaymentVerification auctionPaymentVerification, IAuctionUnlockScheduler auctionUnlockScheduler, ISagaCoordinator sagaCoordinator,
            Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Saga, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _logger = logger;
            _auctionRepository = auctionRepository;
            _auctionPaymentVerification = auctionPaymentVerification;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _sagaCoordinator = sagaCoordinator;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BuyNowCommand> request, Lazy<EventBusFacade> eventBus, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new ArgumentException($"Invalid auction id: {request.Command.AuctionId}");
            }
            _logger.LogDebug($"User {request.Command.SignedInUser} is buying auction {request.Command.AuctionId}");

            var transactionId = await auction.Buy(new UserId(request.Command.SignedInUser), "test", _auctionPaymentVerification, _auctionUnlockScheduler);
            await StartSaga(request, auction, transactionId);
            eventBus.Value.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);

            return RequestStatus.CreatePending(request.CommandContext);
        }

        private async Task StartSaga(AppCommand<BuyNowCommand> request, Auction auction, Guid transactionId)
        {
            var txStartedEvent = (DomainEvents.Events.V1.BuyNowTXStarted)auction.PendingEvents.First(e => e.EventName == "buyNowTXStarted");
            var context = SagaContext
                .Create()
                .WithSagaId(request.CommandContext.CorrelationId.Value)
                .WithMetadata(BuyNowSaga.AuctionContextParamName, auction)
                .WithMetadata(BuyNowSaga.TransactionContextParamName, transactionId)
                .Build();
            await _sagaCoordinator.ProcessAsync(txStartedEvent, context);
        }
    }
}