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
    public class BuyNowTXSuccessSubscriber : EventSubscriber<Events.V1.BuyNowTXSuccess>
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public BuyNowTXSuccessSubscriber(IAppEventBuilder eventBuilder, ImmediateCommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.BuyNowTXSuccess> appEvent)
        {
            var completePaymentCommand = new CompletePaymentCommand()
            {
                UserId = appEvent.Event.BuyerId,
                TransactionId = appEvent.Event.TransactionId,
            };

            await _mediator.Send(completePaymentCommand, appEvent.CommandContext);
        }
    }
}
