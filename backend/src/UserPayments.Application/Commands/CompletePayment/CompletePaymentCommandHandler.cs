using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CompletePayment
{
    public class CompletePaymentCommandHandler : CommandHandlerBase<CompletePaymentCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public CompletePaymentCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository allUserPayments, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allUserPayments = allUserPayments;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CompletePaymentCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

            return await _optimisticConcurrencyHandler.Run(async (repeats, uowFatory) =>
            {
                var payment = userPayments.Payments.FirstOrDefault(p => p.TransactionId.Value == request.Command.TransactionId);
                if (payment is null)
                {
                    return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
                }
                userPayments.CompletePayment(payment.Id);

                using (var uow = uowFatory.Begin())
                {
                    _allUserPayments.Update(userPayments);
                    await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    userPayments.MarkPendingEventsAsHandled();
                    uow.Commit();
                }
                userPayments.MarkPendingEventsAsHandled();
                return RequestStatus.CreateCompleted(request.CommandContext);

            });
        }
    }
}
