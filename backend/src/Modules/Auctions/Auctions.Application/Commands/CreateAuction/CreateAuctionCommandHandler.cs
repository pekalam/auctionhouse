using System;
using Auctions.Application.Sagas;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Commands.Callbacks;
using Common.Application.Events;
using Common.Extensions;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : CommandHandlerBase<CreateAuctionCommand>
    {
        private readonly ILogger<CreateAuctionCommandHandler> _logger;
        private readonly ICategoryNamesToTreeIdsConversion _categoryNamesToTreeIdsConversion;
        private readonly ISagaCoordinator _sagaCoordinator;
        private readonly IUnitOfWorkFactory _uow;
        private readonly IAuctionRepository _auctions;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;
        private readonly IAuctionEndScheduler _auctionEndScheduler;
        private readonly IAuctionImageRepository _auctionImages;


        public CreateAuctionCommandHandler(ILogger<CreateAuctionCommandHandler> logger, ICategoryNamesToTreeIdsConversion categoryNamesToTreeIdsConversion,
            ISagaCoordinator sagaCoordinator, IUnitOfWorkFactory uow, CommandHandlerBaseDependencies dependencies, IAuctionRepository auctions, IAuctionEndScheduler auctionEndScheduler, IAuctionImageRepository auctionImages)
            : base(dependencies)
        {
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
            _logger = logger;
            _categoryNamesToTreeIdsConversion = categoryNamesToTreeIdsConversion;
            _sagaCoordinator = sagaCoordinator;
            _uow = uow;
            _auctions = auctions;
            _auctionEndScheduler = auctionEndScheduler;
            _auctionImages = auctionImages;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var auction = await CreateAuction(request);
            // * actions performed after this call can fail because scheduler call can be ignored when auction
            // is created in invalid state or not persisted
            await _auctionEndScheduler.ScheduleAuctionEnd(auction);
            UpdateAuctionImagesMetadata(request);

            using (var uow = _uow.Begin())
            {
                _auctions.AddAuction(auction);
                await SetExtensionValues(auction);
                await StartCreateAuctionSaga(request, auction);
                await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext);
                auction.MarkPendingEventsAsHandled();
                uow.Commit();
            }

            return RequestStatus.CreatePending(request.CommandContext);
        }

        private void UpdateAuctionImagesMetadata(AppCommand<CreateAuctionCommand> request)
        {
            var imagesToSave = request.Command.AuctionCreateSession.AuctionImages.AllImageIds.ToArray();
            _auctionImages.UpdateManyMetadata(imagesToSave, new AuctionImageMetadata(null)
            {
                IsAssignedToAuction = true
            });
        }

        private async Task<Auction> CreateAuction(AppCommand<CreateAuctionCommand> request)
        {
            var auctionArgs = await CreateAuctionArgs(request.Command, new UserId(request.CommandContext.User!.Value));
            var auction = request.Command.AuctionCreateSession.CreateAuction(auctionArgs);
            return auction;
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
            builder = await builder.SetCategories(request.Category.ToArray(), _categoryNamesToTreeIdsConversion);
            if (request.BuyNowPrice != null)
            {
                builder.SetBuyNow(request.BuyNowPrice.Value);
            }

            return builder.Build();
        }

        private async Task StartCreateAuctionSaga(AppCommand<CreateAuctionCommand> request, Auction auction)
        {
            if (auction.BuyNowOnly)
            {
                return;
            }

            var auctionCreated = (AuctionCreated)auction.PendingEvents.Single(e => e is AuctionCreated);

            var context = SagaContext
                .Create()
                .WithSagaId(request.CommandContext.CorrelationId.Value)
                .Build();
            await _sagaCoordinator.ProcessAsync(auctionCreated, context);
        }

        private Task SetExtensionValues(Auction auction)
        {
            if (!auction.BuyNowOnly)
            {
                return _commandHandlerCallbacks.CallExtension(CommonExtensionKeys.ReadModelNotificationsMode, CommonExtensionKeys.ReadModelNotificationsSagaMode);
            }
            else
            {
                return _commandHandlerCallbacks.CallExtension(CommonExtensionKeys.ReadModelNotificationsMode, CommonExtensionKeys.ReadModelNotificationsImmediateMode);
            }
        }
    }
}