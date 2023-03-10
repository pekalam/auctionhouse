using Auctions.DomainEvents;
using Common.Application.Events;
using Common.Application.Mediator;
using Users.Application.Commands.WithdrawCredits;

namespace Users.Application.EventSubscriptions
{
    public class AuctionBuyConfirmedEventSubscriber : EventSubscriber<Events.V1.AuctionBuyConfirmed>
    {
        private readonly CommandQueryMediator _mediator;

        public AuctionBuyConfirmedEventSubscriber(IAppEventBuilder eventBuilder, CommandQueryMediator mediator) : base(eventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<Events.V1.AuctionBuyConfirmed> appEvent)
        {
            var cmd = new WithdrawCreditsForBuyNowAuctionCommand
            {
                TransactionId = appEvent.Event.BuyerId,
                UserId = appEvent.Event.BuyerId,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
