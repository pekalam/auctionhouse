using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain.Events;

namespace ReadModel.Core.EventConsumers
{
    public class BuyNowPaymentCreatedEventConsumer : EventConsumer<BuyNowPaymentCreated, BuyNowPaymentCreatedEventConsumer>
    {
        public BuyNowPaymentCreatedEventConsumer(ILogger<BuyNowPaymentCreatedEventConsumer> logger, EventConsumerDependencies dependencies) : base(logger, dependencies)
        {
        }

        public override Task Consume(IAppEvent<BuyNowPaymentCreated> appEvent)
        {
            return Task.CompletedTask;
        }
    }
}
