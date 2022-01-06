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
        private readonly IAuctionUnlockScheduler _auctionUnlockScheduler;
        private readonly IAuctionPaymentVerification _auctionPaymentVerification;
        private readonly IAuctionRepository _auctionRepository;
        private readonly EventBusFacade _eventBus;
        private readonly ISagaCoordinator _sagaCoordinator;

        public BuyNowCommandHandler(IAuctionRepository auctionRepository, EventBusFacade eventBusFacade, ILogger<BuyNowCommandHandler> logger, IAuctionPaymentVerification auctionPaymentVerification, IAuctionUnlockScheduler auctionUnlockScheduler, ISagaCoordinator sagaCoordinator) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _eventBus = eventBusFacade;
            _auctionPaymentVerification = auctionPaymentVerification;
            _auctionUnlockScheduler = auctionUnlockScheduler;
            _sagaCoordinator = sagaCoordinator;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<BuyNowCommand> request, CancellationToken cancellationToken)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            if (auction == null)
            {
                throw new ArgumentException($"Invalid auction id: {request.Command.AuctionId}");
            }
            _logger.LogDebug($"User {request.Command.SignedInUser} is buying auction {request.Command.AuctionId}");

            var transactionId = await auction.Buy(new UserId(request.Command.SignedInUser), "test", _auctionPaymentVerification, _auctionUnlockScheduler);
            await StartSaga(request, auction, transactionId);
            _eventBus.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);

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