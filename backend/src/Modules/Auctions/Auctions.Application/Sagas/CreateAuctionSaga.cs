using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application.Commands;
using Common.Application.Mediator;
using Microsoft.Extensions.Logging;
using AuctionBidsEvents = AuctionBids.DomainEvents.Events;

namespace Auctions.Application.Sagas
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
        private readonly CommandQueryMediator _mediator;

        public CreateAuctionSaga(ILogger<CreateAuctionSaga> logger, CommandQueryMediator mediator)
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
}
