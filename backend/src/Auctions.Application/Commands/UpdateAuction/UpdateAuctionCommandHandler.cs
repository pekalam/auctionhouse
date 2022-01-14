using System;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UpdateAuction
{
    public class UpdateAuctionCommandHandler : CommandHandlerBase<UpdateAuctionCommand>
    {
        private readonly IAuctionRepository _auctions;
        private readonly ILogger<UpdateAuctionCommandHandler> _logger;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IConvertCategoryNamesToRootToLeafIds _convertCategoryNamesToIds;

        public UpdateAuctionCommandHandler(IAuctionRepository auctions, ILogger<UpdateAuctionCommandHandler> logger,
            CommandHandlerBaseDependencies dependencies, IUnitOfWorkFactory unitOfWorkFactory, IConvertCategoryNamesToRootToLeafIds convertCategoryNamesToIds)
            : base(ReadModelNotificationsMode.Immediate, dependencies)
        {
            _auctions = auctions;
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
            _convertCategoryNamesToIds = convertCategoryNamesToIds;
        }

        private Auction GetAuction(AppCommand<UpdateAuctionCommand> request)
        {
            var auction = _auctions.FindAuction(request.Command.AuctionId);
            return auction ?? throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
        }


        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<UpdateAuctionCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var auction = GetAuction(request);

            if (!auction.Owner.Value.Equals(request.Command.SignedInUser))
            {
                throw new UnauthorizedAccessException($"User is not owner of an auction {auction.AggregateId}");
            }


            if(request.Command.Tags != null)
            {
                auction.UpdateTags(request.Command.Tags.Select(s => new Tag(s)).ToArray());
            }
            if(request.Command.Name != null)
            {
                auction.UpdateName(request.Command.Name);
            }
            if (request.Command.BuyNowPrice != null)
            {
                auction.UpdateBuyNowPrice(request.Command.BuyNowPrice);
            }
            if(request.Command.Description != null)
            {
                auction.UpdateDescription(request.Command.Description);
            }
            if (request.Command.EndDate != null)
            {
                auction.UpdateEndDate(request.Command.EndDate);
            }
            if(request.Command.Category != null)
            {
                var newCategoryIds = await _convertCategoryNamesToIds.ConvertNames(request.Command.Category.ToArray());
                auction.UpdateCategories(newCategoryIds);
            }

            if(auction.PendingEvents.Count == 0)
            {
                var requestStatus = RequestStatus.CreatePending(request.CommandContext);
                requestStatus.MarkAsCompleted();
                return requestStatus;
            }

            var response = RequestStatus.CreatePending(request.CommandContext);

            using(var uow = _unitOfWorkFactory.Begin())
            {
                _auctions.UpdateAuction(auction);
                await eventOutbox.SaveEvents(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);

                uow.Commit();
            }

            return response;
        }
    }
}