using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Exceptions.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Users;
using Core.Common.SchedulerService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.CreateAuction
{
    public class CreateAuctionCommandHandlerDepedencies
    {
        public IAuctionRepository auctionRepository;
        public IUserIdentityService userIdService;
        public IAuctionSchedulerService auctionSchedulerService;
        public EventBusService eventBusService;
        public ILogger<CreateAuctionCommandHandler> logger;
        public CategoryBuilder categoryBuilder;
        public IUserRepository userRepository;
        public IAuctionCreateSessionService auctionCreateSessionService;
        public IAuctionImageRepository auctionImageRepository;

        public CreateAuctionCommandHandlerDepedencies(IAuctionRepository auctionRepository, IUserIdentityService userIdService, IAuctionSchedulerService auctionSchedulerService, EventBusService eventBusService, ILogger<CreateAuctionCommandHandler> logger, CategoryBuilder categoryBuilder, IUserRepository userRepository, IAuctionCreateSessionService auctionCreateSessionService, IAuctionImageRepository auctionImageRepository)
        {
            this.auctionRepository = auctionRepository;
            this.userIdService = userIdService;
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

    public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand>
    {
        private readonly CreateAuctionCommandHandlerDepedencies _deps;

        public CreateAuctionCommandHandler(CreateAuctionCommandHandlerDepedencies depedencies)
        {
            _deps = depedencies;
            SetupRollbackHandler();
        }

        protected virtual void SetupRollbackHandler() =>
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(CreateAuctionCommand),
                provider => new CreateAuctionRollbackHandler(provider));

        protected virtual void AddToRepository(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                _deps.auctionRepository.AddAuction(auction);
            }
            catch (Exception e)
            {
                _deps.logger.LogWarning($"Cannot add auction to repository {e.Message}");
                throw new CommandException("Cannot add auction to repository", e);
            }
        }

        protected virtual void AddToRepository_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                _deps.auctionRepository.RemoveAuction(auction.AggregateId);
            }
            catch (Exception e)
            {
                _deps.logger.LogCritical($"Cannot rollback add auction to repository {e.Message}");
                throw new CommandException("Cannot rollback add auction to repository", e);
            }
        }

        protected virtual void SheduleAuctionEndTask(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                var scheduledTaskId = _deps.auctionSchedulerService.ScheduleAuctionEndTask(auction)
                    .Result;
                context.TransactionContext = scheduledTaskId;
            }
            catch (Exception e)
            {
                _deps.logger.LogWarning($"Cannot schedule auction end {e.Message}");
                throw new CommandException("Cannot schedule auction end", e);
            }
        }

        protected virtual void ScheduleAuctionEndTask_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                var scheduledTaskId = (ScheduledTaskId) context.TransactionContext;
                _deps.auctionSchedulerService.CancelAuctionEndTask(scheduledTaskId);
            }
            catch (Exception e)
            {
                _deps.logger.LogCritical($"Cannot schedule auction end {e.Message}");
                throw new CommandException("Cannot schedule auction end", e);
            }
        }

        protected virtual void ChangeImagesMetadata(Auction auction, AtomicSequence<Auction> context)
        {
            if (auction.AuctionImages.Count(img => img == null) == auction.AuctionImages.Count)
            {
                return;
            }

            try
            {
                _deps.auctionImageRepository.UpdateManyMetadata(auction.AuctionImages
                    .Where(image => image != null)
                    .SelectMany(img => new string[3] {img.Size1Id, img.Size2Id, img.Size3Id}.AsEnumerable())
                    .ToArray(), new AuctionImageMetadata()
                {
                    IsAssignedToAuction = true
                });
            }
            catch (Exception e)
            {
                _deps.logger.LogError("Cannot set auction images metadata");
                throw;
            }
        }

        protected virtual void ChangeImagesMetadata_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            if (auction.AuctionImages.Count(img => img == null) == auction.AuctionImages.Count)
            {
                return;
            }

            try
            {
                _deps.auctionImageRepository.UpdateManyMetadata(auction.AuctionImages
                    .Where(image => image != null)
                    .SelectMany(img => new string[3] { img.Size1Id, img.Size2Id, img.Size3Id }.AsEnumerable())
                    .ToArray(), new AuctionImageMetadata()
                {
                    IsAssignedToAuction = false
                });
            }
            catch (Exception e)
            {
                _deps.logger.LogCritical("Cannot rollback set auction images metadata");
                throw;
            }
        }

        protected virtual void PublishEvents(Auction auction, User user, CreateAuctionCommand command)
        {
            try
            {
                _deps.eventBusService.Publish(auction.PendingEvents, command.CorrelationId, command);
                //_deps.eventBusService.Publish(user.PendingEvents, command.CorrelationId, command);
                auction.MarkPendingEventsAsHandled();
            }
            catch (Exception e)
            {
                _deps.logger.LogCritical($"Cannot publish aution's pending events");
                throw new CommandException("Cannot publish aution's pending events", e);
            }
        }

        private User GetSignedInUser()
        {
            var userIdentity = _deps.userIdService.GetSignedInUserIdentity();
            if (userIdentity == null)
            {
                throw new UserNotSignedInException("User not signed in");
            }

            var user = _deps.userRepository.FindUser(userIdentity);
            if (user == null)
            {
                throw new DomainException($"Cannot find user {userIdentity.UserName}");
            }

            return user;
        }

        private AuctionArgs GetAuctionArgs(CreateAuctionCommand request, UserIdentity owner)
        {
            var category = _deps.categoryBuilder.FromCategoryNamesList(request.Category);
            var builder = new AuctionArgs.Builder()
                .SetStartDate(request.StartDate)
                .SetEndDate(request.EndDate)
                .SetCategory(category)
                .SetProduct(request.Product)
                .SetTags(request.Tags)
                .SetOwner(owner);
            if (request.BuyNowPrice.HasValue)
            {
                builder.SetBuyNow(request.BuyNowPrice.Value);
            }

            return builder.Build();
        }

        public virtual Task<Unit> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            var user = GetSignedInUser();
            var auctionCreateSession = _deps.auctionCreateSessionService.GetSessionForSignedInUser();

            var auction = auctionCreateSession.CreateAuction(GetAuctionArgs(request, user.UserIdentity));

            _deps.auctionCreateSessionService.RemoveSessionForSignedInUser();

            var addAuctionSequence = new AtomicSequence<Auction>()
                .AddTask(AddToRepository, AddToRepository_Rollback)
                .AddTask(SheduleAuctionEndTask, ScheduleAuctionEndTask_Rollback)
                .AddTask(ChangeImagesMetadata, ChangeImagesMetadata_Rollback)
                .AddTask((param, _) => PublishEvents(auction, user, request), null);

            addAuctionSequence.ExecuteSequence(auction);

            return Task.FromResult(Unit.Value);
        }
    }
}