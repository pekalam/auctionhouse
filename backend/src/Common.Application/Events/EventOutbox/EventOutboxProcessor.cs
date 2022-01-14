using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Common.Application.Events
{
    internal static class SysTime //TODO common for domain and dependent projects
    {
        internal static Func<DateTime> DateTimeFactory { get; set; } = () => DateTime.UtcNow;

        public static DateTime Now => DateTimeFactory();
    }

    public class EventOutboxProcessorSettings
    {
        public long MinMilisecondsDiff { get; set; }
    }

    public class EventOutboxProcessor
    {
        private readonly IOutboxItemFinder _outboxItemFinder;
        private readonly ILogger<EventOutboxProcessor> _logger;
        private readonly EventOutboxProcessorSettings _settings;
        private readonly EventOutboxSender _eventOutboxSender;

        public EventOutboxProcessor(EventOutboxProcessorSettings settings, IOutboxItemFinder outboxItemFinder, ILogger<EventOutboxProcessor> logger, EventOutboxSender eventOutboxSender)
        {
            _settings = settings;
            _outboxItemFinder = outboxItemFinder;
            _logger = logger;
            _eventOutboxSender = eventOutboxSender;
        }

        private static long ToMiliseconds(long timestamp1, long timestamp2) => (timestamp1 - timestamp2) / (1_000_000 / 100);

        private static long MilisecondsToFileTime(long miliseconds) => miliseconds * (1_000_000 / 100);

        private async Task HandleUnprocessedEvents(int total, long timestampDiff, long currentTimestamp)
        {
            _logger.LogDebug("Handling {total} outbox items", total);
            var unprocessedItems = await _outboxItemFinder.GetUnprocessedItemsOlderThan(timestampDiff, currentTimestamp, total);

            _logger.LogDebug("Publishing {total} events from outbox", unprocessedItems.Count()); //TODO 
            await _eventOutboxSender.SendEvents(unprocessedItems);
        }

        public async Task ProcessEvents()
        {
            var currentTimestamp = SysTime.Now.ToFileTime();
            var timestampDiff = MilisecondsToFileTime(_settings.MinMilisecondsDiff);

            var totalUnprocessed = await _outboxItemFinder.GetTotalUnprocessedItemsOlderThan(timestampDiff, currentTimestamp);
            if (totalUnprocessed > 0)
            {
                _logger.LogWarning("Found {total} items to process", totalUnprocessed);
                await HandleUnprocessedEvents(totalUnprocessed, timestampDiff, currentTimestamp);
            }
            else
            {
                _logger.LogDebug("No outbox items to process");
            }
        }
    }
}
