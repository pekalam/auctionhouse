using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.Exceptions;
using Core.Common.SchedulerService;
using Microsoft.Extensions.Logging;

namespace Core.Command.CreateAuction
{
    public class CreateAuctionCommandHandler : CommandHandlerBase<CreateAuctionCommand>
    {
        private readonly CreateAuctionCommandHandlerDepedencies _deps;

        public CreateAuctionCommandHandler(CreateAuctionCommandHandlerDepedencies depedencies) : base(depedencies.logger)
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
                throw new CommandException("Cannot rollback add auction to repository", e);
            }
        }

        protected virtual void SheduleAuctionEndTask(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                var scheduledTaskId = _deps.auctionSchedulerService.ScheduleAuctionEnd(auction)
                    .Result;
                context.TransactionContext = scheduledTaskId;
            }
            catch (Exception e)
            {
                throw new CommandException("Cannot schedule auction end", e);
            }
        }

        protected virtual void ScheduleAuctionEndTask_Rollback(Auction auction, AtomicSequence<Auction> context)
        {
            try
            {
                var scheduledTaskId = (ScheduledTaskId) context.TransactionContext;
                _deps.auctionSchedulerService.CancelAuctionEnd(scheduledTaskId);
            }
            catch (Exception e)
            {
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
                    .ToArray(), new AuctionImageMetadata(null)
                {
                    IsAssignedToAuction = true
                });
            }
            catch (Exception)
            {
                _deps.logger.LogError("Cannot set auction images metadata for an auction: {@auction}", auction);
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
                    .ToArray(), new AuctionImageMetadata(null)
                {
                    IsAssignedToAuction = false
                });
            }
            catch (Exception)
            {
                _deps.logger.LogError("Cannot rollback set auction images metadata");
                throw;
            }
        }

        protected virtual void PublishEvents(Auction auction, User user, AppCommand<CreateAuctionCommand> appCommand)
        {
            try
            {
                _deps.eventBusService.Publish(auction.PendingEvents, appCommand.CommandContext, ReadModelNotificationsMode.Immediate);
                //_deps.eventBusService.Publish(user.PendingEvents, command.CorrelationId, command);
                auction.MarkPendingEventsAsHandled();
            }
            catch (Exception e)
            {
                _deps.logger.LogError($"Cannot publish aution's pending events");
                throw new CommandException("Cannot publish aution's pending events", e);
            }
        }

        private User GetSignedInUser(CreateAuctionCommand cmd)
        {
//            var userIdentity = _deps.userIdService.GetSignedInUserIdentity();
//            if (userIdentity == null)
//            {
//                throw new UserNotSignedInException("User not signed in");
//            }

            var user = _deps.userRepository.FindUser(cmd.SignedInUser);
            if (user == null)
            {
                throw new DomainException($"Cannot find user {cmd.SignedInUser}");
            }

            return user;
        }

        private AuctionArgs GetAuctionArgs(CreateAuctionCommand request, Common.Domain.Auctions.UserId owner)
        {
            var category = _deps.categoryBuilder.FromCategoryNamesList(request.Category);
            var builder = new AuctionArgs.Builder()
                .SetStartDate(request.StartDate)
                .SetEndDate(request.EndDate)
                .SetCategory(category)
                .SetProduct(request.Product)
                .SetTags(request.Tags)
                .SetName(request.Name)
                .SetBuyNowOnly(request.BuyNowOnly.Value)
                .SetOwner(owner);
            if (request.BuyNowPrice != null)
            {
                builder.SetBuyNow(request.BuyNowPrice.Value);
            }

            return builder.Build();
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionCommand> request, CancellationToken cancellationToken)
        {
            var user = GetSignedInUser(request.Command);
            var auction = request.Command.AuctionCreateSession.CreateAuction(GetAuctionArgs(request.Command, new Common.Domain.Auctions.UserId(user.AggregateId)));

            var response = RequestStatus.CreatePending(request.CommandContext);
            var addAuctionSequence = new AtomicSequence<Auction>()
                .AddTask(AddToRepository, AddToRepository_Rollback)
                .AddTask(SheduleAuctionEndTask, ScheduleAuctionEndTask_Rollback)
                .AddTask(ChangeImagesMetadata, ChangeImagesMetadata_Rollback)
                .AddTask((param, _) => PublishEvents(auction, user, request), null);

            addAuctionSequence.ExecuteSequence(auction);

            return Task.FromResult(response);
        }
    }
}