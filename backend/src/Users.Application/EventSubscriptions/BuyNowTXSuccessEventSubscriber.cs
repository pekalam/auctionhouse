using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Commands.WithdrawCredits;

namespace Users.Application.EventSubscriptions
{
    public class BuyNowTXSuccessEventSubscriber : EventSubscriber<Events.V1.BuyNowTXSuccess>
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public BuyNowTXSuccessEventSubscriber(IAppEventBuilder eventBuilder, ImmediateCommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.BuyNowTXSuccess> appEvent)
        {
            var cmd = new WithdrawCreditsForBuyNowAuctionCommand
            {
                TransactionId = appEvent.Event.TransactionId,
                UserId = appEvent.Event.BuyerId,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
