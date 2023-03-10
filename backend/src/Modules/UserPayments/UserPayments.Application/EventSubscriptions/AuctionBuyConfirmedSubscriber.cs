using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Application.Commands.CompletePayment;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application.EventSubscriptions
{
    public class AuctionBuyConfirmedSubscriber : EventSubscriber<Events.V1.AuctionBuyConfirmed>
    {
        private readonly CommandQueryMediator _mediator;

        public AuctionBuyConfirmedSubscriber(IAppEventBuilder eventBuilder, CommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.AuctionBuyConfirmed> appEvent)
        {
            var completePaymentCommand = new CompletePaymentCommand()
            {
                UserId = appEvent.Event.BuyerId,
                TransactionId = appEvent.Event.BuyerId,
            };

            await _mediator.Send(completePaymentCommand, appEvent.CommandContext);
        }
    }
}
