using Microsoft.Extensions.Hosting;
using RabbitMq.EventBus;
using System.Reflection;

namespace Adatper.RabbitMq.EventBus
{
    internal class RabbitMqSubscriptionInitializer : IHostedService
    {
        private readonly Assembly[]? _eventSubscriptionAssemblies;
        private readonly Assembly[]? _eventConsumerAssemblies;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqSubscriptionInitializer(Assembly[]? eventSubscriptionAssemblies, Assembly[]? eventConsumerAssemblies, IServiceProvider serviceProvider)
        {
            _eventSubscriptionAssemblies = eventSubscriptionAssemblies;
            _eventConsumerAssemblies = eventConsumerAssemblies;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken ct)
        {
            if (_eventConsumerAssemblies != null)
            {
                RabbitMqInstaller.InitializeEventConsumers(_serviceProvider, _eventConsumerAssemblies);
            }
            if (_eventSubscriptionAssemblies != null)
            {
                RabbitMqInstaller.InitializeEventSubscriptions(_serviceProvider, _eventSubscriptionAssemblies);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
