using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : CommandHandlerBase<CreateAuctionCommand>
    {
        public readonly ILogger<CreateAuctionCommandHandler> _logger;
        private readonly IConvertCategoryNamesToRootToLeafIds _convertCategoryNames;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly CreateAuctionService _createAuctionService;
        private readonly EventBusFacade _eventBusFacade;


        public CreateAuctionCommandHandler(ILogger<CreateAuctionCommandHandler> logger,
            IConvertCategoryNamesToRootToLeafIds convertCategoryNames, ISagaCoordinator sagaCoordinator,
            CreateAuctionService createAuctionService, EventBusFacade eventBusFacade) : base(logger)
        {
            _logger = logger;
            _convertCategoryNames = convertCategoryNames;
            _sagaCoordinator = sagaCoordinator;
            _createAuctionService = createAuctionService;
            _eventBusFacade = eventBusFacade;
        }

        private async Task<AuctionArgs> CreateAuctionArgs(CreateAuctionCommand request, UserId owner)
        {
            var builder = new AuctionArgs.Builder()
                .SetStartDate(request.StartDate)
                .SetEndDate(request.EndDate)
                .SetProduct(request.Product)
                .SetTags(request.Tags)
                .SetName(request.Name)
                .SetBuyNowOnly(request.BuyNowOnly)
                .SetOwner(owner);
            builder = await builder.SetCategories(request.Category.ToArray(), _convertCategoryNames);
            if (request.BuyNowPrice != null)
            {
                builder.SetBuyNow(request.BuyNowPrice.Value);
            }

            return builder.Build();
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionCommand> request, CancellationToken cancellationToken)
        {
            var auctionArgs = await CreateAuctionArgs(request.Command, new UserId(request.CommandContext.User!.Value));
            var auction = await _createAuctionService.StartCreate(request.Command.AuctionCreateSession, auctionArgs);
            _createAuctionService.Commit();


            if (!_createAuctionService.Finished) // when transaction is not finished it means that distributed transaction must be started
            {
                var createdEvent = (AuctionCreated)auction.PendingEvents.FirstOrDefault(t => t.EventName == "auctionCreated");

                if(createdEvent is null)
                {
                    throw new Exception();
                }

                var context = SagaContext
                    .Create()
                    .WithSagaId(request.CommandContext.CorrelationId.Value)
                    .WithMetadata(CreateAuctionSaga.ServiceDataKey, _createAuctionService.ServiceData)
                    .Build();
                await _sagaCoordinator.ProcessAsync(createdEvent, context);
                _eventBusFacade.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
            }
            else
            {
                _eventBusFacade.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
            }
            auction.MarkPendingEventsAsHandled();


            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}