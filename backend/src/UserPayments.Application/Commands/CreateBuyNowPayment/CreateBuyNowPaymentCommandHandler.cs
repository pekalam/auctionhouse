using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateBuyNowPayment
{
    using Common.Application.Events;
    using Domain;
    using Domain.Shared;
    using UserPayments.DomainEvents;

    public class CreateBuyNowPaymentCommandHandler : CommandHandlerBase<CreateBuyNowPaymentCommand>
    {
        private readonly IUserPaymentsRepository _userPayments;
        private readonly EventBusFacade _eventBus;

        public CreateBuyNowPaymentCommandHandler(ILogger<CreateBuyNowPaymentCommandHandler> logger, IUserPaymentsRepository userPayments, EventBusFacade eventBus) : base(logger)
        {
            _userPayments = userPayments;
            _eventBus = eventBus;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateBuyNowPaymentCommand> request, CancellationToken cancellationToken)
        {
            var userPayments = await _userPayments.WithUserId(new UserId(request.Command.BuyerId));

            var payment = userPayments.CreateBuyNowPayment(new TransactionId(request.Command.TransactionId),
                new UserId(request.Command.BuyerId), request.Command.Amount, paymentTargetId: new PaymentTargetId(request.Command.AuctionId));
            _eventBus.Publish(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
            userPayments.MarkPendingEventsAsHandled();

            //test
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await Task.Delay(3000);

                userPayments.ConfirmPayment(payment.Id);

                _eventBus.Publish(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                _eventBus.Publish(new Events.V1.BuyNowPaymentConfirmed
                {
                    TransactionId = request.Command.TransactionId,
                }, request.CommandContext, ReadModelNotificationsMode.Saga);
                userPayments.MarkPendingEventsAsHandled();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}
