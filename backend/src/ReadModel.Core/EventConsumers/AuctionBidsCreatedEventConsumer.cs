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
        public AuctionBidsCreatedEventConsumer(IAppEventBuilder appEventBuilder, ILogger<AuctionBidsCreatedEventConsumer> logger, Lazy<ISagaNotifications> sagaNotificationsFactory, Lazy<IImmediateNotifications> immediateNotifications) : base(appEventBuilder, logger, sagaNotificationsFactory, immediateNotifications)
        {
        }

        public override void Consume(IAppEvent<AuctionBidsCreated> appEvent)
        {
        }
    }
}
