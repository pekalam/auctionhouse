using System;
using Auctions.Application.Sagas;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.BuyNow
{
    public class BuyNowCommandHandler : CommandHandlerBase<BuyNowCommand>
    {
        private readonly ILogger<BuyNowCommandHandler> _logger;
        private readonly IAuctionBuyCancellationScheduler _auctionBuyCancellationScheduler;
        private readonly IAuctionPaymentVerification _auctionPaymentVerification;
        private readonly IAuctionRepository _auctions;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public BuyNowCommandHandler(IAuctionRepository auctions, ILogger<BuyNowCommandHandler> logger,
            IAuctionPaymentVerification auctionPaymentVerification, IAuctionBuyCancellationScheduler auctionBuyCancellationScheduler, ISagaCoordinator sagaCoordinator,
            CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(dependencies)
        {
            _logger = logger;
            _auctions = auctions;
            _auctionPaymentVerification = auctionPaymentVerification;
            _auctionBuyCancellationScheduler = auctionBuyCancellationScheduler;
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

                await auction.Buy(new UserId(request.Command.SignedInUser), "test", _auctionPaymentVerification, _auctionBuyCancellationScheduler);

                using (var uow = uowFactory.Begin())
                {
                    _auctions.UpdateAuction(auction);
                    await StartSaga(request, auction, auction.Buyer);
                    await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                    uow.Commit();
                }
                auction.MarkPendingEventsAsHandled();
            });


            return RequestStatus.CreatePending(request.CommandContext);
        }

        private async Task StartSaga(AppCommand<BuyNowCommand> request, Auction auction, Guid transactionId)
        {
            var txStartedEvent = (DomainEvents.Events.V1.AuctionBought)auction.PendingEvents.First(e => e.EventName == "auctionBought");
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