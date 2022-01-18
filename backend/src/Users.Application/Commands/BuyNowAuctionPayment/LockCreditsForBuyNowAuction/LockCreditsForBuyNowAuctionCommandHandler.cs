using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain.Users;
using Core.DomainFramework;
using Users.Domain;
using Users.Domain.Repositories;
using Users.DomainEvents;

namespace Users.Application.Commands.ChargeUser
{
    public class LockCreditsForBuyNowAuctionCommandHandler : CommandHandlerBase<LockCreditsForBuyNowAuctionCommand>
    {
        private readonly IUserRepository _users;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public LockCreditsForBuyNowAuctionCommandHandler(CommandHandlerBaseDependencies dependencies, IUserRepository users,
            OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _users = users;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<LockCreditsForBuyNowAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var user = _users.FindUser(new UserId(request.Command.UserId));
            if (user is null)
            {
                throw new ArgumentException("Could not find user with id " + request.Command.UserId);
            }

            return await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) user = _users.FindUser(new UserId(request.Command.UserId));

                if (!TryLockCredits(request, user))
                {
                    await eventOutbox.SaveEvent(new UserCreditsFailedToLockForBuyNowAuction
                    {
                        TransactionId = request.Command.TransactionId,
                        UserId = request.Command.UserId,
                    }, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
                }

                using (var uow = uowFactory.Begin())
                {
                    _users.UpdateUser(user);
                    await eventOutbox.SaveEvents(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    await eventOutbox.SaveEvent(new UserCreditsLockedForBuyNowAuction
                    {
                        TransactionId = request.Command.TransactionId,
                        UserId = request.Command.UserId,
                    }, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    uow.Commit();
                }
                user.MarkPendingEventsAsHandled();

                return RequestStatus.CreateCompleted(request.CommandContext);
            });
        }

        private static bool TryLockCredits(AppCommand<LockCreditsForBuyNowAuctionCommand> request, User user)
        {
            try
            {
                user.LockCredits(new(request.Command.TransactionId), request.Command.Amount);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }
    }
}
