using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using Core.Common.SchedulerService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.UpdateAuction
{
    public class UpdateAuctionCommand : ICommand, IRequest
    {
        public Guid AuctionId { get; }

        //optional
        public decimal? BuyNowPrice { get; }
        public DateTime? EndDate { get; }
        public List<string> Category { get; }
        public string Description { get; }
        public string[] Tags { get; }
        public string Name { get; }


        public CorrelationId CorrelationId { get; }
    }

    public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<UpdateAuctionCommandHandler> _logger;
        private readonly IAuctionSchedulerService _schedulerService;
        private readonly EventBusService _eventBusService;
        private readonly CategoryBuilder _categoryBuilder;

        public UpdateAuctionCommandHandler(IAuctionRepository auctionRepository, IUserIdentityService userIdentityService, ILogger<UpdateAuctionCommandHandler> logger, IAuctionSchedulerService schedulerService, EventBusService eventBusService, CategoryBuilder categoryBuilder)
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

        public Task<Unit> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
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

            return Task.FromResult(Unit.Value);
        }
    }
}