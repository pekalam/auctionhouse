using Common.Application.Events;

namespace Common.Application.Commands
{
    public class CommandContext
    {
        public CommandId CommandId { get; set; }
        public CorrelationId CorrelationId { get; set; }
        public Guid User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; set; }

        public static CommandContext CreateHttpQueued(Guid user, string name) => new CommandContext { HttpQueued = true, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = CorrelationId.CreateNew(), WSQueued = false };
        public static CommandContext CreateNew(Guid user, string name) => new CommandContext { HttpQueued = false, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = CorrelationId.CreateNew(), WSQueued = false };
        public static CommandContext CreateWSQueued(Guid user, string name) => new CommandContext { HttpQueued = false, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = CorrelationId.CreateNew(), WSQueued = true };
    }
}
