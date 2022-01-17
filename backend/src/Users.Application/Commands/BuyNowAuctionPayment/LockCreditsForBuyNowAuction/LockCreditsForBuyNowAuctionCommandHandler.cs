using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain.Users;
using Core.DomainFramework;
using Users.Domain;
using Users.Domain.Repositories;
using Users.Domain.Shared;
using Users.DomainEvents;

namespace Users.Application.Commands.ChargeUser
{
    public class LockCreditsForBuyNowAuctionCommandHandler : CommandHandlerBase<LockCreditsForBuyNowAuctionCommand>
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public LockCreditsForBuyNowAuctionCommandHandler(CommandHandlerBaseDependencies dependencies, IUserRepository users, IUnitOfWorkFactory unitOfWorkFactory) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _users = users;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<LockCreditsForBuyNowAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var user = _users.FindUser(new UserId(request.Command.UserId));

            if (user is null)
            {
                throw new ArgumentException("Could not find user with id " + request.Command.UserId);
            }

            try
            {
                user.LockCredits(new LockedFundsId(request.Command.TransactionId), request.Command.Amount);
            }
            catch (DomainException)
            {
                await eventOutbox.SaveEvent(new UserCreditsFailedToLockForBuyNowAuction
                {
                    TransactionId = request.Command.TransactionId,
                    UserId = request.Command.UserId,
                }, request.CommandContext, ReadModelNotificationsMode.Disabled);
                return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
            }

            using (var uow = _unitOfWorkFactory.Begin())
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
        }
    }
}
