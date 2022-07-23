using Common.Application.Events;

namespace Common.Application.Commands
{
    public class CommandContext
    {
        public CommandId CommandId { get; init; }
        public CorrelationId CorrelationId { get; init; }
        public Guid? User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; init; }

        public CommandContext(CommandId commandId, CorrelationId correlationId, Guid? user, bool httpQueued, bool wsQueued, string name)
        {
            CommandId = commandId;
            CorrelationId = correlationId;
            User = user;
            HttpQueued = httpQueued;
            WSQueued = wsQueued;
            Name = name;
        }

        public static CommandContext CreateNew(string name, Guid? user = null) => new(CommandId.CreateNew(), CorrelationId.CreateNew(), user, false, false, name);
    }
}
