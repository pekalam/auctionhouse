using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Command;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Infrastructure.Services.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IntegrationTests
{
    public class QueuedCommandBase : ICommand    {
        public int X { get; set; }
    }


    public class RabbitMqEventBusTestUtils
    {
        public static Lazy<RabbitMqEventBus> Bus { get; } = new Lazy<RabbitMqEventBus>(() =>
        {
            var bus = new RabbitMqEventBus(new RabbitMqSettings()
            {
                ConnectionString =
                    TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqEventBus>>());
            return bus;
        });

        public static Lazy<RabbitMqQueuedCommandBus> QueuedCommandBus { get; } = new Lazy<RabbitMqQueuedCommandBus> (() => {

            var bus = new RabbitMqQueuedCommandBus(new RabbitMqSettings()
            {
                ConnectionString =
                        TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost"),
            }, Mock.Of<ILogger<RabbitMqQueuedCommandBus>>());
            return bus;
        });
    }

    public class RabbitMqEventsBus_QueuedCommandHandling_Tests
    {



       
    }
}