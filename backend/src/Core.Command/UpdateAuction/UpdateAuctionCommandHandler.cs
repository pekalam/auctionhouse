using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Exceptions.Command;
using Core.Common.SchedulerService;
using Microsoft.Extensions.Logging;

namespace Core.Command.UpdateAuction
{
    public class UnauthorizedAccessException : CommandException
    {
        public UnauthorizedAccessException(string message) : base(message)
        {
        }

        public UnauthorizedAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UpdateAuctionCommandHandler : CommandHandlerBase<UpdateAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<UpdateAuctionCommandHandler> _logger;
        private readonly IAuctionSchedulerService _schedulerService;
        private readonly EventBusService _eventBusService;
        private readonly CategoryBuilder _categoryBuilder;

        public UpdateAuctionCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdentityService, 
            ILogger<UpdateAuctionCommandHandler> logger, IAuctionSchedulerService schedulerService, 
            EventBusService eventBusService, CategoryBuilder categoryBuilder) : base(logger)
        {
            _auctionRepository = auctionRepository;
            _userIdentityService = userIdentityService;
            _logger = logger;
            _schedulerService = schedulerService;
            _eventBusService = eventBusService;
            _categoryBuilder = categoryBuilder;
        }

        private Auction GetAuction(UpdateAuctionCommand request)
        {
            var auction = _auctionRepository.FindAuction(request.AuctionId);
            return auction ?? throw new CommandException($"Cannot find auction {request.AuctionId}");
        }


        protected override Task<CommandResponse> HandleCommand(UpdateAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = GetAuction(request);

            if (!auction.Owner.UserId.Equals(request.SignedInUser.UserId))
            {
                throw new UnauthorizedAccessException($"User is not owner of an auction {auction.AggregateId}");
            }

            
            auction.UpdateTags(request.Tags.Select(s => new Tag(s)).ToArray());
            auction.UpdateName(request.Name);
            auction.UpdateBuyNowPrice(request.BuyNowPrice);
            auction.UpdateDescription(request.Description);
            if (request.EndDate != null)
            {
                auction.UpdateEndDate(request.EndDate);
            }
            var newCategory = _categoryBuilder.FromCategoryNamesList(request.Category);
            auction.UpdateCategory(newCategory);

            _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
            _auctionRepository.UpdateAuction(auction);

            var response = new CommandResponse(request.CorrelationId, Status.PENDING);
            return Task.FromResult(response);
        }
    }
}