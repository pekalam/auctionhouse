using System;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : CommandHandlerBase<CreateAuctionCommand>
    {
        public readonly ILogger<CreateAuctionCommandHandler> _logger;
        private readonly IConvertCategoryNamesToRootToLeafIds _convertCategoryNames;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly CreateAuctionService _createAuctionService;
        private readonly IUnitOfWorkFactory _uow;
        private readonly IAuctionRepository _auctions;


        public CreateAuctionCommandHandler(ILogger<CreateAuctionCommandHandler> logger, IConvertCategoryNamesToRootToLeafIds convertCategoryNames,
            ISagaCoordinator sagaCoordinator, CreateAuctionService createAuctionService, IUnitOfWorkFactory uow, CommandHandlerBaseDependencies dependencies, IAuctionRepository auctions)
            : base(ReadModelNotificationsMode.Saga, dependencies)
        {
            _logger = logger;
            _convertCategoryNames = convertCategoryNames;
            _sagaCoordinator = sagaCoordinator;
            _createAuctionService = createAuctionService;
            _uow = uow;
            _auctions = auctions;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var auctionArgs = await CreateAuctionArgs(request.Command, new UserId(request.CommandContext.User!.Value));
            var auction = await _createAuctionService.StartCreate(request.Command.AuctionCreateSession, auctionArgs);

            using (var uowCtx = _uow.Begin())
            {
                _auctions.AddAuction(auction);
                if (!_createAuctionService.Finished) // when transaction is not finished it means that bids must be created and assigned by saga
                {
                    await StartCreateAuctionSaga(request, auction);
                    await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                }
                else
                {
                    await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
                }
                auction.MarkPendingEventsAsHandled();
                uowCtx.Commit();
            }

            return RequestStatus.CreatePending(request.CommandContext);
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

        private async Task StartCreateAuctionSaga(AppCommand<CreateAuctionCommand> request, Auction auction)
        {
            var createdEvent = auction.PendingEvents.FirstOrDefault(t => t.EventName == "auctionCreated") as AuctionCreated;

            if (createdEvent is null)
            {
                throw new Exception();
            }

            var context = SagaContext
                .Create()
                .WithSagaId(request.CommandContext.CorrelationId.Value)
                .WithMetadata(CreateAuctionSaga.ServiceDataKey, _createAuctionService.ServiceData)
                .Build();
            await _sagaCoordinator.ProcessAsync(createdEvent, context);
        }
    }
}