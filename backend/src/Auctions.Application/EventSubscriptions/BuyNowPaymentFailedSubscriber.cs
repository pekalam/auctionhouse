using Chronicle;
using Common.Application.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UserPayments.DomainEvents.Events.V1;

namespace Auctions.Application.EventSubscriptions
{
    public class BuyNowPaymentFailedSubscriber : EventSubscriber<BuyNowPaymentFailed>
    {
        private readonly ISagaCoordinator _sagaCoordinator;

        public BuyNowPaymentFailedSubscriber(IAppEventBuilder eventBuilder, ISagaCoordinator sagaCoordinator) : base(eventBuilder)
        {
            _sagaCoordinator = sagaCoordinator;
        }

        public override Task Handle(IAppEvent<BuyNowPaymentFailed> appEvent)
        {
            return Task.CompletedTask;
        }
    }
}
