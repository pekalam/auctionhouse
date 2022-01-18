using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.SetPaymentStatusToFailed
{
    public class SetPaymentStatusToFailedCommandHandler : CommandHandlerBase<SetPaymentStatusToFailedCommand>
    {
        private readonly IUserPaymentsRepository _allUserPayments;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public SetPaymentStatusToFailedCommandHandler(CommandHandlerBaseDependencies dependencies, IUserPaymentsRepository allUserPayments, OptimisticConcurrencyHandler optimisticConcurrencyHandler) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allUserPayments = allUserPayments;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<SetPaymentStatusToFailedCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

            return await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if(repeats > 0) userPayments = await _allUserPayments.WithUserId(new Domain.Shared.UserId(request.Command.UserId));

                var payment = userPayments.Payments.FirstOrDefault(p => p.TransactionId.Value == request.Command.TransactionId);
                if (payment is null)
                {
                    return RequestStatus.CreateFromCommandContext(request.CommandContext, Status.FAILED);
                }
                userPayments.SetPaymentToFailed(payment.Id);

                using (var uow = uowFactory.Begin())
                {
                    _allUserPayments.Update(userPayments);
                    await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                    uow.Commit();
                }
                userPayments.MarkPendingEventsAsHandled();
                return RequestStatus.CreateCompleted(request.CommandContext);
            });
        }
    }
}
