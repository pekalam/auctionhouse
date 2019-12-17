using Core.Common.ApplicationServices;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Users;
using Core.Common.SchedulerService;
using Microsoft.Extensions.Logging;

namespace Core.Command.CreateAuction
{
    public class CreateAuctionCommandHandlerDepedencies
    {
        public IAuctionRepository auctionRepository;
        public IAuctionSchedulerService auctionSchedulerService;
        public EventBusService eventBusService;
        public ILogger<CreateAuctionCommandHandler> logger;
        public CategoryBuilder categoryBuilder;
        public IUserRepository userRepository;
        public IAuctionCreateSessionService auctionCreateSessionService;
        public IAuctionImageRepository auctionImageRepository;

        public CreateAuctionCommandHandlerDepedencies(IAuctionRepository auctionRepository, IAuctionSchedulerService auctionSchedulerService, EventBusService eventBusService, ILogger<CreateAuctionCommandHandler> logger, CategoryBuilder categoryBuilder, IUserRepository userRepository, IAuctionCreateSessionService auctionCreateSessionService, IAuctionImageRepository auctionImageRepository)
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