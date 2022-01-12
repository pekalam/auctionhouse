using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;
using AuctionBidsEvents = AuctionBids.DomainEvents.Events;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionSagaData
    {
        public CreateAuctionServiceData CreateAuctionServiceData { get; set; } = null!;
    }

    public class CreateAuctionSaga : Saga<CreateAuctionSagaData>, ISagaStartAction<AuctionCreated>, ISagaAction<AuctionBidsEvents.V1.AuctionBidsCreated>
    {
        public const string ServiceDataKey = "ServiceData";
        public const string CorrelationIdKey = "CorrelationId";
        private readonly Lazy<IAuctionImageRepository> _auctionImages;
        private readonly Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private readonly Lazy<IAuctionRepository> _auctions;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly ILogger<CreateAuctionSaga> _logger;

        public CreateAuctionSaga(Lazy<IAuctionImageRepository> auctionImages, Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox, ILogger<CreateAuctionSaga> logger)
        {
            _auctionImages = auctionImages;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            _sagaNotifications = sagaNotifications;
            _logger = logger;
        }

        public Task CompensateAsync(AuctionCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task CompensateAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(AuctionCreated message, ISagaContext context)
        {
            if (!context.TryGetMetadata(ServiceDataKey, out var metadata))
            {
                throw new InvalidOperationException();
            }
            Data.CreateAuctionServiceData = (CreateAuctionServiceData)metadata.Value;
            return Task.CompletedTask;
        }

        public async Task HandleAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            var correlationId = (CorrelationId)context.GetMetadata(CorrelationIdKey).Value;
            var createAuctionService = new CreateAuctionService(_auctionImages, _auctionEndScheduler, _auctions, Data.CreateAuctionServiceData);
            createAuctionService.EndCreate(new Domain.AuctionBidsId(message.AuctionBidsId));
            createAuctionService.Commit();
            await _sagaNotifications.Value.MarkSagaAsCompleted(correlationId);
            await CompleteAsync();
        }
    }
}
