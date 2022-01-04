using Auctions.Domain;
using Auctions.Domain.Repositories;
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
        private readonly IAuctionRepository _auctions;
        private readonly IConvertCategoryNamesToRootToLeafIds _convertCategoryNames;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly CreateAuctionService _createAuctionService;
        private readonly EventBusFacade _eventBusFacade;
        private readonly IAuctionEndScheduler _auctionEndScheduler;
        private readonly IAuctionImageRepository _auctionImages;


        public CreateAuctionCommandHandler(ILogger<CreateAuctionCommandHandler> logger, IAuctionRepository auctions,
            IConvertCategoryNamesToRootToLeafIds convertCategoryNames, ISagaCoordinator sagaCoordinator,
            CreateAuctionService createAuctionService, EventBusFacade eventBusFacade, IAuctionEndScheduler auctionEndScheduler, 
            IAuctionImageRepository auctionImages) : base(logger)
        {
            _logger = logger;
            _auctions = auctions;
            _convertCategoryNames = convertCategoryNames;
            _sagaCoordinator = sagaCoordinator;
            _createAuctionService = createAuctionService;
            _eventBusFacade = eventBusFacade;
            _auctionEndScheduler = auctionEndScheduler;
            _auctionImages = auctionImages;
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
            var auction = _createAuctionService.StartCreate(request.Command.AuctionCreateSession, auctionArgs);

            _auctionImages.UpdateManyMetadata(auction.AuctionImages //TODO move to auction session
                    .Where(image => image != null)
                    .SelectMany(img => new string[3] { img.Size1Id, img.Size2Id, img.Size3Id }.AsEnumerable())
                    .ToArray(), new AuctionImageMetadata(null)
                    {
                        IsAssignedToAuction = true
                    });

            // if further instructions will fail then request from a schedule service can be ignored
            await _auctionEndScheduler.ScheduleAuctionEnd(auction);

            _auctions.AddAuction(auction);
            var context = SagaContext
                .Create()
                .WithSagaId(request.CommandContext.CorrelationId.Value)
                .WithMetadata(CreateAuctionSaga.ServiceDataKey, _createAuctionService.ServiceData)
                .Build();
            await _sagaCoordinator.ProcessAsync((AuctionCreated)auction.PendingEvents.First(), context);

            _eventBusFacade.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);

            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}