using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Users.Domain.Repositories;

namespace Users.Application.Commands.SignUp.AssignUserPayments
{
    public class AssignUserPaymentsCommandHandler : CommandHandlerBase<AssignUserPaymentsCommand>
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;

        public AssignUserPaymentsCommandHandler(IUserRepository users, IUnitOfWorkFactory unitOfWorkFactory,
            CommandHandlerBaseDependencies dependencies) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _users = users;
            _unitOfWorkFactory = unitOfWorkFactory;
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<AssignUserPaymentsCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var user = _users.FindUser(new(request.Command.UserId));
            if (user is null)
            {
                return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
            }

            user.AssignUserPayments(new(request.Command.UserPaymentsId));
            if (user.PendingEvents.Count == 0) return RequestStatus.CreateCompleted(request.CommandContext); //idempotency

            using var uow = _unitOfWorkFactory.Begin();
            _users.UpdateUser(user);
            await eventOutbox.SaveEvents(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
            //await _sagaNotifications.AddUnhandledEvents(request.CommandContext.CorrelationId, user.PendingEvents); //TODO this should be explicit
            await _commandHandlerCallbacks.OnUowCommit(request);
            user.MarkPendingEventsAsHandled();
            uow.Commit();
           

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
