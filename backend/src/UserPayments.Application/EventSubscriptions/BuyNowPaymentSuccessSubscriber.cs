using Auctions.DomainEvents;
using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.EventSubscriptions
{
    public class BuyNowPaymentSuccessSubscriber : EventSubscriber<Events.V1.BuyNowTXSuccess>
    {
        private readonly EventBusFacade _eventBus;
        private readonly IUserPaymentsRepository _userPayments;

        public BuyNowPaymentSuccessSubscriber(IAppEventBuilder eventBuilder, EventBusFacade eventBus, IUserPaymentsRepository userPayments) : base(eventBuilder)
        {
            _eventBus = eventBus;
            _userPayments = userPayments;
        }

        public override async Task Handle(IAppEvent<Events.V1.BuyNowTXSuccess> appEvent)
        {
            var userPayments = await _userPayments.WithUserId(new Domain.Shared.UserId(appEvent.Event.BuyerId));

            var payment = userPayments.Payments.First(p => p.TransactionId.Value == appEvent.Event.TransactionId);
            userPayments.ConfirmPayment(payment.Id);
            _eventBus.Publish(userPayments.PendingEvents, appEvent.CommandContext, ReadModelNotificationsMode.Saga);
            userPayments.MarkPendingEventsAsHandled();
        }
    }
}
