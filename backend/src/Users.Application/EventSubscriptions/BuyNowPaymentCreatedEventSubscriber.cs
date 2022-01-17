using Common.Application.Events;
using Common.Application.Mediator;
using UserPayments.Domain.Events;
using Users.Application.Commands.ChargeUser;

namespace Users.Application.EventSubscriptions
{
    public class BuyNowPaymentCreatedEventSubscriber : EventSubscriber<BuyNowPaymentCreated>
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public BuyNowPaymentCreatedEventSubscriber(IAppEventBuilder eventBuilder, ImmediateCommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<BuyNowPaymentCreated> appEvent)
        {
            var cmd = new LockCreditsForBuyNowAuctionCommand
            {
                Amount = appEvent.Event.Amount,
                PaymentId = appEvent.Event.PaymentId,
                UserId = appEvent.Event.UserId,
                TransactionId = appEvent.Event.TransactionId,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
