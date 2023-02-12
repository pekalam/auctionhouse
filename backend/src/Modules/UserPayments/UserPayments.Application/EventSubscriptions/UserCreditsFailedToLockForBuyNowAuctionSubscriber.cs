using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Application.EventSubscriptions
{
    using Common.Application.Mediator;
    using UserPayments.Application.Commands.CompletePayment;
    using UserPayments.Application.Commands.SetPaymentStatusToFailed;
    using UserPayments.DomainEvents;
    using Users.DomainEvents;

    public class UserCreditsFailedToLockForBuyNowAuctionSubscriber : EventSubscriber<UserCreditsFailedToLockForBuyNowAuction>
    {
        private readonly CommandQueryMediator _mediator;


        public UserCreditsFailedToLockForBuyNowAuctionSubscriber(IAppEventBuilder eventBuilder, CommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<UserCreditsFailedToLockForBuyNowAuction> appEvent)
        {
            var cmd = new SetPaymentStatusToFailedCommand
            {
                UserId = appEvent.Event.UserId,
                TransactionId = appEvent.Event.TransactionId,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
