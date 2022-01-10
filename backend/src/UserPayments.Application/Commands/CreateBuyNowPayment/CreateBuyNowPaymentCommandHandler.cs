using System;
using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.Commands.CreateBuyNowPayment
{
    using Common.Application.Events;
    using Common.Application.SagaNotifications;
    using Domain;
    using Domain.Shared;
    using UserPayments.DomainEvents;

    public class CreateBuyNowPaymentCommandHandler : CommandHandlerBase<CreateBuyNowPaymentCommand>
    {
        private readonly IUserPaymentsRepository _userPayments;

        public CreateBuyNowPaymentCommandHandler(ILogger<CreateBuyNowPaymentCommandHandler> logger, IUserPaymentsRepository userPayments,
            Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox) 
            : base(ReadModelNotificationsMode.Disabled, logger, immediateNotifications, sagaNotifications, eventBusFacadeWithOutbox)
        {
            _userPayments = userPayments;
        }

        protected override async Task<RequestStatus> HandleCommand(
            AppCommand<CreateBuyNowPaymentCommand> request, Lazy<EventBusFacade> eventBus,
            CancellationToken cancellationToken)
        {
            var userPayments = await _userPayments.WithUserId(new UserId(request.Command.BuyerId));

            var payment = userPayments.CreateBuyNowPayment(new TransactionId(request.Command.TransactionId),
                new UserId(request.Command.BuyerId), request.Command.Amount, paymentTargetId: new PaymentTargetId(request.Command.AuctionId));
            eventBus.Value.Publish(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
            userPayments.MarkPendingEventsAsHandled();

            //test
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await Task.Delay(3000);

                userPayments.ConfirmPayment(payment.Id);

                eventBus.Value.Publish(userPayments.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Disabled);
                eventBus.Value.Publish(new Events.V1.BuyNowPaymentConfirmed
                {
                    TransactionId = request.Command.TransactionId,
                }, request.CommandContext, ReadModelNotificationsMode.Saga);
                ((EventBusFacadeWithOutbox)eventBus.Value).ProcessOutbox();
                userPayments.MarkPendingEventsAsHandled();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return RequestStatus.CreatePending(request.CommandContext);
        }
    }
}
