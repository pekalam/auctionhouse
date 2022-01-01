using Auctions.Domain;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UpdateAuction
{
    public class UpdateAuctionCommandHandler : CommandHandlerBase<UpdateAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<UpdateAuctionCommandHandler> _logger;
        private readonly IAuctionEndScheduler _schedulerService;
        private readonly EventBusFacade _eventBusService;

        public UpdateAuctionCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdentityService,
            ILogger<UpdateAuctionCommandHandler> logger, IAuctionEndScheduler schedulerService,
            EventBusFacade eventBusService) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _userIdentityService = userIdentityService;
            _logger = logger;
            _schedulerService = schedulerService;
            _eventBusService = eventBusService;
        }

        private Auction GetAuction(AppCommand<UpdateAuctionCommand> request)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            return auction ?? throw new InvalidOperationException($"Cannot find auction {request.Command.AuctionId}");
        }


        protected override Task<RequestStatus> HandleCommand(AppCommand<UpdateAuctionCommand> request, CancellationToken cancellationToken)
        {
            var auction = GetAuction(request);

            //if (!auction.Owner.Equals(request.Command.SignedInUser))
            //{
            //    throw new UnauthorizedAccessException($"User is not owner of an auction {auction.AggregateId}");
            //}


            //auction.UpdateTags(request.Command.Tags.Select(s => new Tag(s)).ToArray());
            //auction.UpdateName(request.Command.Name);
            //auction.UpdateBuyNowPrice(request.Command.BuyNowPrice);
            //auction.UpdateDescription(request.Command.Description);
            //if (request.Command.EndDate != null)
            //{
            //    auction.UpdateEndDate(request.Command.EndDate);
            //}
            //var newCategory = _categoryBuilder.FromCategoryNamesList(request.Command.Category);
            //auction.UpdateCategory(newCategory);

            var response = RequestStatus.CreatePending(request.CommandContext);
            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);

            return Task.FromResult(response);
        }
    }
}