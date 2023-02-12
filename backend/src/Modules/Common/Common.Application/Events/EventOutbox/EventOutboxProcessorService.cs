using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Common.Application.Events
{
    internal class EventOutboxProcessorService : BackgroundService
    {
        private readonly ILogger<EventOutboxProcessorService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EventOutboxProcessorService(IServiceScopeFactory serviceScopeFactory, ILogger<EventOutboxProcessorService> logger, EventOutboxProcessorSettings eventOutboxProcessorSettings)
        {
            _logger = eventOutboxProcessorSettings.EnableLogging ? logger : new NullLogger<EventOutboxProcessorService>();
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    _logger.LogDebug("Processing events from outbox");
                    var eventOutboxProcessor = scope.ServiceProvider.GetRequiredService<EventOutboxProcessor>();
                    try
                    {
                        await eventOutboxProcessor.ProcessEvents(); //TODO pass cancellation token in all calls
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while processing events");
                        throw;
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
