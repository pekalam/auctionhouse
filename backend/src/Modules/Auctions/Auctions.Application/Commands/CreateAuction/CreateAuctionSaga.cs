using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Mediator;
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

        private readonly ILogger<CreateAuctionSaga> _logger;
        private readonly ImmediateCommandQueryMediator _mediator;

        public CreateAuctionSaga(ILogger<CreateAuctionSaga> logger, ImmediateCommandQueryMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
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

        public async Task HandleAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            var cmd = new EndCreateAuctionCommand
            {
                AuctionBidsId = message.AuctionBidsId,
                CreateAuctionServiceData = Data.CreateAuctionServiceData
            };
            await _mediator.Send(cmd, GetCommandContext(context));
            await CompleteAsync();
        }
    }

    public class EndCreateAuctionCommand : ICommand
    {
        public Guid AuctionBidsId { get; set; }

        public CreateAuctionServiceData CreateAuctionServiceData { get; set; }
    }

    public class EndCreateAuctionCommandHandler : CommandHandlerBase<EndCreateAuctionCommand>
    {
        private readonly Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private readonly Lazy<IAuctionRepository> _auctions;
        private readonly Lazy<IAuctionImageRepository> _auctionImages;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventOutbox _eventOutbox;

        public EndCreateAuctionCommandHandler(CommandHandlerBaseDependencies dependencies,
            Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions,
            Lazy<IAuctionImageRepository> auctionImages, IUnitOfWorkFactory unitOfWorkFactory) :
            base(ReadModelNotificationsMode.Saga, dependencies)
        {
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
            _eventOutbox = dependencies.EventOutbox;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
            _auctionImages = auctionImages;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<EndCreateAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var createAuctionService = new CreateAuctionService(_auctionImages, _auctionEndScheduler, _auctions, request.Command.CreateAuctionServiceData);
            var auction = createAuctionService.EndCreate(new Domain.AuctionBidsId(request.Command.AuctionBidsId));

            //TODO rm:
            // auctionBidsAdded doesn't need to be projected in read model
            var eventsToSend = auction.PendingEvents.Where(e => e.EventName != "auctionBidsAdded");
            // there is no need to handle concurrency issues
            using (var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.Value.UpdateAuction(auction);
                await _eventOutbox.SaveEvents(eventsToSend, request.CommandContext, ReadModelNotificationsMode.Saga);
                await _commandHandlerCallbacks.OnUowCommit(request);
                uow.Commit();
            }

            auction.MarkPendingEventsAsHandled();
            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
