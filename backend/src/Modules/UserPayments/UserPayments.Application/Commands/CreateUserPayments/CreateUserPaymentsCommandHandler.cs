using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateUserPayments
{
    using Core.DomainFramework;
    using UserPayments.Domain;

    public class CreateUserPaymentsCommandHandler : CommandHandlerBase<CreateUserPaymentsCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CreateUserPaymentsCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository userPaymentsRepository, IUnitOfWorkFactory unitOfWorkFactory) : base(dependencies)
        {
            _allUserPayments = userPaymentsRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateUserPaymentsCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var existing = await _allUserPayments.WithUserId(new(request.Command.UserId));
            // try to detect if already called - happy path
            if(existing is not null) 
            {
                return RequestStatus.CreateCompleted(request.CommandContext);
            }

            var userPayments = UserPayments.CreateNew(new(request.Command.UserId));

            using(var uow = _unitOfWorkFactory.Begin())
            {
                if (!TryAddUserPayments(userPayments)) // try to detect if already called - pesimistic path (handling db exceptions)
                {
                    return RequestStatus.CreateCompleted(request.CommandContext);
                }
                await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext);
                uow.Commit();
            }
            userPayments.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }

        private bool TryAddUserPayments(UserPayments userPayments)
        {
            try
            {
                _allUserPayments.Add(userPayments);
                return true;
            }
            catch (ConcurrentInsertException e)
            {
                return false;
            }
        }
    }
}
