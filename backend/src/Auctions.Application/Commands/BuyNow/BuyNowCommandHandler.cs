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
        private readonly IAuctionRepository _auctions;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public BuyNowCommandHandler(IAuctionRepository auctions, ILogger<BuyNowCommandHandler> logger,
            IAuctionPaymentVerification auctionPaymentVerification, IAuctionUnlockScheduler auctionUnlockScheduler, ISagaCoordinator sagaCoordinator,
            CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(ReadModelNotificationsMode.Saga, dependencies)
        {
            _logger = logger;
            _auctions = auctions;
            _auctionPaymentVerification = auctionPaymentVerification;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _sagaCoordinator = sagaCoordinator;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BuyNowCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new ArgumentException($"Invalid auction id: {request.Command.AuctionId}");
            }
            _logger.LogDebug($"User {request.Command.SignedInUser} is buying auction {request.Command.AuctionId}");

            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) => {
                if (repeats > 0) auction = _auctions.FindAuction(request.Command.AuctionId);

                var transactionId = await auction.Buy(new UserId(request.Command.SignedInUser), "test", _auctionPaymentVerification, _auctionUnlockScheduler);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.UpdateAuction(auction);
                    await StartSaga(request, auction, transactionId);
                    await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    auction.MarkPendingEventsAsHandled();
                    uow.Commit();
                }
            });


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