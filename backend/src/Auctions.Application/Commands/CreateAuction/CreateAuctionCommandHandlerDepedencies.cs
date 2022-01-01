using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application.Events;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionCommandHandlerDepedencies
    {
        public IAuctionRepository auctionRepository;
        public IAuctionEndScheduler auctionSchedulerService;
        public EventBusFacade eventBusService;
        public ILogger<CreateAuctionCommandHandler> logger;
        public CategoryBuilder categoryBuilder;
        public IUserRepository userRepository;
        public IAuctionCreateSessionStore auctionCreateSessionService;
        public IAuctionImageRepository auctionImageRepository;

        public CreateAuctionCommandHandlerDepedencies(IAuctionRepository auctionRepository, IAuctionEndScheduler auctionSchedulerService, EventBusFacade eventBusService, ILogger<CreateAuctionCommandHandler> logger, CategoryBuilder categoryBuilder, IUserRepository userRepository, IAuctionCreateSessionStore auctionCreateSessionService, IAuctionImageRepository auctionImageRepository)
        {
            this.auctionRepository = auctionRepository;
            this.auctionSchedulerService = auctionSchedulerService;
            this.eventBusService = eventBusService;
            this.logger = logger;
            this.categoryBuilder = categoryBuilder;
            this.userRepository = userRepository;
            this.auctionCreateSessionService = auctionCreateSessionService;
            this.auctionImageRepository = auctionImageRepository;
        }

        public CreateAuctionCommandHandlerDepedencies()
        {

        }
    }
}