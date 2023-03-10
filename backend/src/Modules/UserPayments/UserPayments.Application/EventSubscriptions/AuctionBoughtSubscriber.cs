using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.Mediator;
using UserPayments.Application.Commands.CreateBuyNowPayment;

namespace UserPayments.Application.EventSubscriptions
{
    public class AuctionBoughtSubscriber : EventSubscriber<Events.V1.AuctionBought>
    {
        private readonly CommandQueryMediator _mediator;

        public AuctionBoughtSubscriber(IAppEventBuilder eventBuilder, CommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.AuctionBought> appEvent)
        {
            var cmd = new CreateBuyNowPaymentCommand
            {
                AuctionId = appEvent.Event.AuctionId,
                BuyerId = appEvent.Event.BuyerId,
                TransactionId = appEvent.Event.BuyerId,
                PaymentMethodName = appEvent.Event.PaymentMethodName,
                Amount = appEvent.Event.Price,
            };
            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
