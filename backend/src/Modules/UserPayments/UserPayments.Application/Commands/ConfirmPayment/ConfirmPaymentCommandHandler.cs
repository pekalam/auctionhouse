using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.ConfirmPayment
{
    public class ConfirmPaymentCommandHandler : CommandHandlerBase<ConfirmPaymentCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public ConfirmPaymentCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository allUserPayments,
            OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(dependencies)
        {
            _allUserPayments = allUserPayments;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<ConfirmPaymentCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

            return await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

                var payment = userPayments.Payments.FirstOrDefault(p => p.TransactionId.Value == request.Command.TransactionId);
                if (payment is null)
                {
                    return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
                }
                userPayments.ConfirmPayment(payment.Id);

                using (var uow = uowFactory.Begin())
                {
                    _allUserPayments.Update(userPayments);
                    await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext);
                    userPayments.MarkPendingEventsAsHandled();
                    uow.Commit();
                }
                userPayments.MarkPendingEventsAsHandled();
                return RequestStatus.CreateCompleted(request.CommandContext);
            });
        }
    }
}
