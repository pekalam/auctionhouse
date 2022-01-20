using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateUserPayments
{
    using UserPayments.Domain;

    public class CreateUserPaymentsCommandHandler : CommandHandlerBase<CreateUserPaymentsCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CreateUserPaymentsCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository userPaymentsRepository, IUnitOfWorkFactory unitOfWorkFactory) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allUserPayments = userPaymentsRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateUserPaymentsCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var existing = await _allUserPayments.WithUserId(new(request.Command.UserId));

            if(existing is not null)
            {
                return RequestStatus.CreateCompleted(request.CommandContext);
            }

            var userPayments = UserPayments.CreateNew(new(request.Command.UserId));

            using(var uow = _unitOfWorkFactory.Begin())
            {
                _allUserPayments.Add(userPayments);
                await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                uow.Commit();
            }
            userPayments.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
