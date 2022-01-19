using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
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
        public const string CommandContextKey = "CommandContext";

        private readonly Lazy<IAuctionImageRepository> _auctionImages;
        private readonly Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private readonly Lazy<IAuctionRepository> _auctions;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly ILogger<CreateAuctionSaga> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly Lazy<IEventOutbox> _eventOutbox;
        private readonly Lazy<EventOutboxSender> _eventOutboxSender;

        public CreateAuctionSaga(Lazy<IAuctionImageRepository> auctionImages, Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions,
            Lazy<ISagaNotifications> sagaNotifications, ILogger<CreateAuctionSaga> logger, IUnitOfWorkFactory unitOfWorkFactory, Lazy<IEventOutbox> eventOutbox, Lazy<EventOutboxSender> eventOutboxSender)
        {
            _auctionImages = auctionImages;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            _sagaNotifications = sagaNotifications;
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
            _eventOutbox = eventOutbox;
            _eventOutboxSender = eventOutboxSender;
        }

        private CommandContext GetCommandContext(ISagaContext context)
        {
            return (CommandContext)context.GetMetadata(CommandContextKey).Value;
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

        private async Task TrySendEvents(OutboxItem[] outboxItems)
        {
            try
            {
                await _eventOutboxSender.Value.SendEvents(outboxItems!);
            }
            catch (Exception)
            {
            }
        }

        public async Task HandleAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            var correlationId = (CorrelationId)context.GetMetadata(CorrelationIdKey).Value;
            var createAuctionService = new CreateAuctionService(_auctionImages, _auctionEndScheduler, _auctions, Data.CreateAuctionServiceData);
            var auction = createAuctionService.EndCreate(new Domain.AuctionBidsId(message.AuctionBidsId));

            OutboxItem[] savedOutboxItems;
            // there is no need to handle concurrency issues
            using(var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.Value.UpdateAuction(auction);
                savedOutboxItems = await _eventOutbox.Value.SaveEvents(auction.PendingEvents, GetCommandContext(context), ReadModelNotificationsMode.Saga);
                await _sagaNotifications.Value.MarkSagaAsCompleted(correlationId);
                uow.Commit();
            }
            await TrySendEvents(savedOutboxItems);

            auction.MarkPendingEventsAsHandled();
            await CompleteAsync();
        }
    }
}
