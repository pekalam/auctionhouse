using Auctions.DomainEvents;
using Common.Application.Events;

namespace AuctionBids.Application.EventSubscriptions
{
    using AuctionBids.Application.Commands.CreateAuctionBids;
    using Common.Application.Mediator;

    public class AuctionCreatedSubscriber : EventSubscriber<AuctionCreated>
    {
        private readonly ImmediateCommandQueryMediator _mediator;

        public AuctionCreatedSubscriber(IAppEventBuilder appEventBuilder, ImmediateCommandQueryMediator mediator) : base(appEventBuilder)
        {
            _mediator = mediator;
        }

        public override async Task Handle(IAppEvent<AuctionCreated> appEvent)
        {
            var cmd = new CreateAuctionBidsCommand
            {
                AuctionId = appEvent.Event.AuctionId,
                CategoryId = appEvent.Event.Category[0],
                SubCategoryId = appEvent.Event.Category[1],
                SubSubCategoryId = appEvent.Event.Category[2],
                Owner = appEvent.Event.Owner,
            };

            await _mediator.Send(cmd, appEvent.CommandContext);
        }
    }
}
