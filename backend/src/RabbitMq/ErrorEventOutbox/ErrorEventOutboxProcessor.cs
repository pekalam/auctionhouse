using Common.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMq.EventBus;
using System.Text;

namespace Adatper.RabbitMq.EventBus.ErrorEventOutbox
{
    internal class ErrorEventOutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly EventBusSettings _eventBusSettings;
        private readonly ILogger<ErrorEventOutboxProcessor> _logger;

        public ErrorEventOutboxProcessor(IServiceScopeFactory serviceScopeFactory, EventBusSettings eventBusSettings, ILogger<ErrorEventOutboxProcessor> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _eventBusSettings = eventBusSettings;
            _logger = logger;
        }

        private int? GetRedeliveryCount(JToken jToken)
        {
            if(jToken == null || jToken["RedeliveryCount"] == null)
            {
                return null;
            }

            var redeliveryCount = jToken["RedeliveryCount"]!.ToObject<int>();
            return redeliveryCount;
        }

        private string IncrementRedeliveryCount(JToken jToken, int currentRedeliveryCount)
        {
            (jToken["RedeliveryCount"] as JValue)!.Value = currentRedeliveryCount + 1;
            var json = JsonConvert.SerializeObject(jToken);
            return json;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessEvents(stoppingToken);

                await Task.Delay(500, stoppingToken);
            }
        }

        internal async Task ProcessEvents(CancellationToken ct)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var errorEventOutboxStore = scope.ServiceProvider.GetRequiredService<IErrorEventOutboxStore>();
            var errorEventOutboxUnprocesseItems = scope.ServiceProvider.GetRequiredService<IErrorEventOutboxUnprocessedItemsFinder>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IRabbitMqEventBus>();

            var unprocessedItems = errorEventOutboxUnprocesseItems.FindUnprocessed(10);
            if(unprocessedItems.Length > 0)
            {
                _logger.LogInformation("Found {@length} outbox items to process", unprocessedItems.Length);
            }
            foreach (var item in unprocessedItems)
            {
                var jToken = TryParse(item);
                if (jToken is null)
                {
                    _logger.LogError("Could not parse outbox item {messageBody}, deleting from outbox", item.MessageJson);
                    TryDeleteItem(errorEventOutboxStore, item);
                    continue;
                }

                var currentRedeliveryCount = GetRedeliveryCount(jToken);

                if (!currentRedeliveryCount.HasValue)
                {
                    _logger.LogError("Could not parse outbox item redelivery count");
                    TryDeleteItem(errorEventOutboxStore, item);
                    continue;
                }

                if (currentRedeliveryCount == _eventBusSettings.MaxRedelivery)
                {
                    _logger.LogError("Message with routing key {@routingKey} reached max redelivery count. Body: {messageBody}", item.RoutingKey, item.MessageJson);
                    TryDeleteItem(errorEventOutboxStore, item);
                    continue;
                }

                var messageJson = IncrementRedeliveryCount(jToken, currentRedeliveryCount.Value);
                _logger.LogInformation("Redelivering {@routingKey}", item.RoutingKey);
                if (item.MessageProperties.DeliveryMode != 0) //fixes invalid messages 
                {
                    await eventBus.Bus.Advanced.PublishAsync(eventBus.EventExchange, item.RoutingKey,
                        true, item.MessageProperties, Encoding.UTF8.GetBytes(messageJson), ct);
                }


                TryDeleteItem(errorEventOutboxStore, item);
            }
        }

        private void TryDeleteItem(IErrorEventOutboxStore errorEventOutboxStore, ErrorEventOutboxItem item)
        {
            try
            {
                errorEventOutboxStore.Delete(item);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not delete item in {nameof(IErrorEventOutboxStore)}");
            }
        }

        private static JToken? TryParse(ErrorEventOutboxItem item)
        {
            try
            {
                return JToken.Parse(item.MessageJson);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
