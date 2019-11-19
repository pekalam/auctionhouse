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
            var signedInUser = _userIdentityService.GetSignedInUserIdentity();
            if (signedInUser == null)
            {
                throw new CommandException("User not signed in");
            }

            var auction = GetAuction(request);
            if (request.Description != null)
            {
                auction.Product.Description = request.Description;
            }

            if (request.Tags != null)
            {
                auction.SetTags(request.Tags.Select(s => new Tag(s)).ToArray());
            }

            if (request.Name != null)
            {
                auction.SetName(request.Name);
            }

            //TODO
            auction.SetBuyNowPrice(request.BuyNowPrice);


            if (request.EndDate.HasValue)
            {
                auction.SetEndDate(request.EndDate);
                //TODO
            }

            if (request.Category != null)
            {
                var newCategory = _categoryBuilder.FromCategoryNamesList(request.Category);
                auction.SetCategory(newCategory);
            }

            _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
            _auctionRepository.UpdateAuction(auction);

            var response = new CommandResponse(request.CorrelationId, Status.PENDING);
            return Task.FromResult(response);
        }
    }
}