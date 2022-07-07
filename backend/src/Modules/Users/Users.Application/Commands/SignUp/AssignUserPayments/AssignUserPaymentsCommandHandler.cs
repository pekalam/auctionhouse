using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Users.Domain.Repositories;

namespace Users.Application.Commands.SignUp.AssignUserPayments
{
    public class AssignUserPaymentsCommandHandler : CommandHandlerBase<AssignUserPaymentsCommand>
    {
        private readonly IUserRepository _users;
        private readonly ISagaNotifications _sagaNotifications;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public AssignUserPaymentsCommandHandler(IUserRepository users, ISagaNotifications sagaNotifications, IUnitOfWorkFactory unitOfWorkFactory, 
            CommandHandlerBaseDependencies dependencies) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _users = users;
            _sagaNotifications = sagaNotifications;
            _unitOfWorkFactory = unitOfWorkFactory;
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
            await _sagaNotifications.MarkSagaAsCompleted(request.CommandContext.CorrelationId);
            user.MarkPendingEventsAsHandled();
            uow.Commit();
           

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
