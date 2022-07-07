using Common.Application.Events;
using Common.Application.Mediator;
using UserPayments.Application.Commands.ConfirmPayment;
using Users.DomainEvents;

namespace UserPayments.Application.EventSubscriptions
{
    public class UserCreditsLockedForBuyNowAuctionSubscriber : EventSubscriber<UserCreditsLockedForBuyNowAuction>
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public UserCreditsLockedForBuyNowAuctionSubscriber(IAppEventBuilder eventBuilder, ImmediateCommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<UserCreditsLockedForBuyNowAuction> appEvent)
        {
            var cmd = new ConfirmPaymentCommand
            {
                UserId = appEvent.Event.UserId,
                TransactionId = appEvent.Event.TransactionId,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
