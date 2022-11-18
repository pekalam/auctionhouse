using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateBuyNowPayment
{
    using Common.Application.Events;
    using Domain;
    using Domain.Shared;

    public class CreateBuyNowPaymentCommandHandler : CommandHandlerBase<CreateBuyNowPaymentCommand>
    {
        private readonly IUserPaymentsRepository _userPayments;
        private readonly OptimisticConcurrencyHandler _optimisticConcurrencyHandler;

        public CreateBuyNowPaymentCommandHandler(ILogger<CreateBuyNowPaymentCommandHandler> logger, IUserPaymentsRepository userPayments,
            CommandHandlerBaseDependencies dependencies, OptimisticConcurrencyHandler optimisticConcurrencyHandler)
            : base(dependencies)
        {
            _userPayments = userPayments;
            _optimisticConcurrencyHandler = optimisticConcurrencyHandler;
        }

        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<CreateBuyNowPaymentCommand> request, IEventOutbox eventOutbox,
            CancellationToken cancellationToken)
        {
            var userPayments = await _userPayments.WithUserId(new UserId(request.Command.BuyerId));

            await _optimisticConcurrencyHandler.Run(async (repeats, uowFactory) =>
            {
                if (repeats > 0) userPayments = await _userPayments.WithUserId(new UserId(request.Command.BuyerId));

                var payment = userPayments.CreateBuyNowPayment(new TransactionId(request.Command.TransactionId),
                                request.Command.Amount, new(request.Command.PaymentMethodName), paymentTargetId: new PaymentTargetId(request.Command.AuctionId));

                using (var uow = uowFactory.Begin())
                {
                    _userPayments.Update(userPayments);
                    await eventOutbox.SaveEvents(userPayments.PendingEvents, request.CommandContext);
                    uow.Commit();
                }
            });
            userPayments.MarkPendingEventsAsHandled();

            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}
