using Common.Application.Commands;
using Common.Application.Events;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Adapter.SqlServer.EventOutbox
{
    internal class DbOutboxItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string Event { get; set; } = null!;
        public DbCommandContext CommandContext { get; set; } = null!;
        public long Timestamp { get; set; }
        public bool Processed { get; set; }
    }

    internal class DbCommandContext
    {
        public string CommandId { get; set; }
        public string CorrelationId { get; set; }
        public Guid? User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; set; }
        public string? ExtraData { get; set; }
    }

    internal static class DbOutboxItemAssembler
    {
        public static DbOutboxItem ToDbOutboxStoreItem(OutboxItem outboxStoreItem)
        {
            return new DbOutboxItem
            {
                Id = outboxStoreItem.Id,
                Event = SerializationUtils.ToJson(outboxStoreItem.Event),
                CommandContext = new DbCommandContext
                {
                    CommandId = outboxStoreItem.CommandContext.CommandId.Id,
                    CorrelationId = outboxStoreItem.CommandContext.CorrelationId.Value,
                    HttpQueued = outboxStoreItem.CommandContext.HttpQueued,
                    Name = outboxStoreItem.CommandContext.Name,
                    User = outboxStoreItem.CommandContext.User,
                    WSQueued = outboxStoreItem.CommandContext.WSQueued,
                    ExtraData = JsonSerializer.Serialize(outboxStoreItem.CommandContext.ExtraData)
                },
                Timestamp = outboxStoreItem.Timestamp,
                Processed = outboxStoreItem.Processed,
            };
        }

        public static OutboxItem FromDbOutboxStoreItem(DbOutboxItem dbItem)
        {
            return new OutboxItem
            {
                Id = dbItem.Id,
                CommandContext = new CommandContext(
                    new CommandId(dbItem.CommandContext.CommandId),
                    new CorrelationId(dbItem.CommandContext.CorrelationId),
                    dbItem.CommandContext.User,
                    dbItem.CommandContext.HttpQueued,
                    dbItem.CommandContext.WSQueued,
                    dbItem.CommandContext.Name,
                    DeserializeExtraData(dbItem)
                ),
                Timestamp = dbItem.Timestamp,
                Event = SerializationUtils.FromJson(dbItem.Event),
                Processed = dbItem.Processed,
            };
        }

        private static Dictionary<string, string>? DeserializeExtraData(DbOutboxItem outboxItem)
        {
            return outboxItem.CommandContext.ExtraData is null ? null
                : JsonSerializer.Deserialize<Dictionary<string, string>>(outboxItem.CommandContext.ExtraData) ?? throw new ArgumentException("ExtraData deserialization error");
        }
    }
}