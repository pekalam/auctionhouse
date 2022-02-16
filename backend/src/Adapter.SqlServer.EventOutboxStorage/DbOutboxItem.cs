using Common.Application.Commands;
using Common.Application.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adapter.SqlServer.EventOutbox
{
    internal class DbOutboxItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Event { get; set; } = null!;
        public CommandContext CommandContext { get; set; } = null!;
        public ReadModelNotificationsMode ReadModelNotifications { get; set; }
        public long Timestamp { get; set; }
        public bool Processed { get; set; }
    }

    internal static class DbOutboxItemAssembler
    {
        public static DbOutboxItem ToDbOutboxStoreItem(OutboxItem outboxStoreItem)
        {
            return new DbOutboxItem
            {
                Id = outboxStoreItem.Id,
                Event = SerializationUtils.ToJson(outboxStoreItem.Event),
                CommandContext = new CommandContext
                {
                    CommandId = new CommandId(outboxStoreItem.CommandContext.CommandId.Id),
                    CorrelationId = new CorrelationId(outboxStoreItem.CommandContext.CorrelationId.Value),
                    HttpQueued = outboxStoreItem.CommandContext.HttpQueued,
                    Name = outboxStoreItem.CommandContext.Name,
                    User = outboxStoreItem.CommandContext.User,
                    WSQueued = outboxStoreItem.CommandContext.WSQueued,
                },
                ReadModelNotifications = outboxStoreItem.ReadModelNotifications,
                Timestamp = outboxStoreItem.Timestamp,
                Processed = outboxStoreItem.Processed,
            };
        }

        public static OutboxItem FromDbOutboxStoreItem(DbOutboxItem dbItem)
        {
            return new OutboxItem
            {
                Id = dbItem.Id,
                CommandContext = dbItem.CommandContext,
                Timestamp = dbItem.Timestamp,
                ReadModelNotifications = dbItem.ReadModelNotifications,
                Event = SerializationUtils.FromJson(dbItem.Event),
                Processed = dbItem.Processed,
            };
        }
    }
}