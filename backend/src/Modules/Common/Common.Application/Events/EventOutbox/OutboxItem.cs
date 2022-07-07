using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public class OutboxItem
    {
        public long Id { get; set; }
        public CommandContext CommandContext { get; set; }
        public Event Event { get; set; }
        public ReadModelNotificationsMode ReadModelNotifications { get; set; }
        public long Timestamp { get; set; }
        public bool Processed { get; set; }

        public static OutboxItem CreateNew(IAppEvent<Event> appEvent, bool processed)
        {
            return new OutboxItem
            {
                Event = appEvent.Event,
                CommandContext = appEvent.CommandContext,
                ReadModelNotifications = appEvent.ReadModelNotifications,
                Timestamp = SysTime.Now.ToFileTime(),
                Processed = processed,
            };
        }
    }
}
