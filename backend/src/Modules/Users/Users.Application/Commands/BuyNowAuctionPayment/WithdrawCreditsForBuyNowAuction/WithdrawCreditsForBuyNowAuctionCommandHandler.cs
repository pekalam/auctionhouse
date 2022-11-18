using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain.Users;
using Users.Domain;
using Users.Domain.Repositories;

namespace Users.Application.Commands.WithdrawCredits
{
    public class WithdrawCreditsForBuyNowAuctionCommandHandler : CommandHandlerBase<WithdrawCreditsForBuyNowAuctionCommand>
    {
        private readonly IUserRepository _users;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public WithdrawCreditsForBuyNowAuctionCommandHandler(CommandHandlerBaseDependencies dependencies, IUserRepository users,
            OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(dependencies)
        {
            _users = users;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<WithdrawCreditsForBuyNowAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var user = _users.FindUser(new UserId(request.Command.UserId));

            if (user is null)
            {
                throw new ArgumentException("Could not find user with id " + request.Command.UserId);
            }

            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                user.WithdrawCredits(new LockedFundsId(request.Command.TransactionId));

                using (var uow = uowFactory.Begin())
                {
                    _users.UpdateUser(user);
                    await eventOutbox.SaveEvents(user.PendingEvents, request.CommandContext);
                    uow.Commit();
                }
                user.MarkPendingEventsAsHandled();


            });
            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
