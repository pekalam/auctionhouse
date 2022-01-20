using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.SchedulerService;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.UpdateAuction
{
    public class UpdateAuctionCommandHandler : CommandHandlerBase<UpdateAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<UpdateAuctionCommandHandler> _logger;
        private readonly IAuctionEndScheduler _schedulerService;
        private readonly EventBusService _eventBusService;
        private readonly CategoryBuilder _categoryBuilder;

        public UpdateAuctionCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdentityService, 
            ILogger<UpdateAuctionCommandHandler> logger, IAuctionEndScheduler schedulerService, 
            EventBusService eventBusService, CategoryBuilder categoryBuilder) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _userIdentityService = userIdentityService;
            _logger = logger;
            _schedulerService = schedulerService;
            _eventBusService = eventBusService;
            _categoryBuilder = categoryBuilder;
        }

        private Auction GetAuction(AppCommand<UpdateAuctionCommand> request)
        {
            var auction = _auctionRepository.FindAuction(request.Command.AuctionId);
            return auction ?? throw new CommandException($"Cannot find auction {request.Command.AuctionId}");
        }


        protected override Task<RequestStatus> HandleCommand(AppCommand<UpdateAuctionCommand> request, CancellationToken cancellationToken)
        {
            var auction = GetAuction(request);

            if (!auction.Owner.Value.Equals(request.Command.SignedInUser.Value))
            {
                throw new UnauthorizedAccessException($"User is not owner of an auction {auction.AggregateId}");
            }

            
            auction.UpdateTags(request.Command.Tags.Select(s => new Tag(s)).ToArray());
            auction.UpdateName(request.Command.Name);
            auction.UpdateBuyNowPrice(request.Command.BuyNowPrice);
            auction.UpdateDescription(request.Command.Description);
            if (request.Command.EndDate != null)
            {
                auction.UpdateEndDate(request.Command.EndDate);
            }
            var newCategory = _categoryBuilder.FromCategoryNamesList(request.Command.Category);
            auction.UpdateCategory(newCategory);

            var response = RequestStatus.CreatePending(request.CommandContext);
            _auctionRepository.UpdateAuction(auction);
            _eventBusService.Publish(auction.PendingEvents, request.CommandContext, Common.EventBus.ReadModelNotificationsMode.Immediate);

            return Task.FromResult(response);
        }
    }
}