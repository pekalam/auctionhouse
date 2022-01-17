using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuctionBids.DomainEvents.Events.V1;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionBidsCreatedEventConsumer : EventConsumer<AuctionBidsCreated, AuctionBidsCreatedEventConsumer>
    {
        public AuctionBidsCreatedEventConsumer(ILogger<AuctionBidsCreatedEventConsumer> logger, EventConsumerDependencies dependencies) : base(logger, dependencies)
        {
        }

        public override async Task Consume(IAppEvent<AuctionBidsCreated> appEvent)
        {
        }
    }
}
