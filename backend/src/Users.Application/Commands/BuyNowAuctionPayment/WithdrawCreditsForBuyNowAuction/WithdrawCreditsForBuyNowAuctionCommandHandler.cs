using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Core.Common.Domain.Users;
using Core.DomainFramework;
using Users.Domain;
using Users.Domain.Repositories;
using Users.Domain.Shared;

namespace Users.Application.Commands.WithdrawCredits
{
    public class WithdrawCreditsForBuyNowAuctionCommandHandler : CommandHandlerBase<WithdrawCreditsForBuyNowAuctionCommand>
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public WithdrawCreditsForBuyNowAuctionCommandHandler(CommandHandlerBaseDependencies dependencies, IUserRepository users, IUnitOfWorkFactory unitOfWorkFactory) 
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _users = users;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<WithdrawCreditsForBuyNowAuctionCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var user = _users.FindUser(new UserId(request.Command.UserId));

            if(user is null)
            {
                throw new ArgumentException("Could not find user with id " + request.Command.UserId);
            }

            try
            {
                user.WithdrawCredits(new LockedFundsId(request.Command.TransactionId));
            }
            catch (DomainException)
            {
                return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED));
            }
        
            using(var uow = _unitOfWorkFactory.Begin())
            {
                _users.UpdateUser(user);
                eventOutbox.SaveEvents(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                uow.Commit();
            }
            user.MarkPendingEventsAsHandled();

            return Task.FromResult(RequestStatus.CreateCompleted(request.CommandContext));
        }
    }
}
