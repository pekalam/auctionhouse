using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.SetPaymentStatusToFailed
{
    public class SetPaymentStatusToFailedCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid TransactionId { get; set; }
    }

    public class SetPaymentStatusToFailedCommandHandler : CommandHandlerBase<SetPaymentStatusToFailedCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public SetPaymentStatusToFailedCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository allUserPayments, IUnitOfWorkFactory unitOfWorkFactory) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allUserPayments = allUserPayments;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<SetPaymentStatusToFailedCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

            var payment = userPayments.Payments.FirstOrDefault(p => p.TransactionId.Value == request.Command.TransactionId);
            if(payment is null)
            {
                return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
            }
            userPayments.SetPaymentToFailed(payment.Id);

            using (var uow = _unitOfWorkFactory.Begin())
            {
                _allUserPayments.Update(userPayments);
                await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                userPayments.MarkPendingEventsAsHandled();
                uow.Commit();
            }
            userPayments.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
