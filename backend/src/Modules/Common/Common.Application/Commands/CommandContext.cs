using Common.Application.Events;

namespace Common.Application.Commands
{
    public class CommandContext
    {
        public CommandId CommandId { get; set; }
        public CorrelationId CorrelationId { get; set; }
        public Guid? User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; set; }

        public static CommandContext CreateNew(string name, Guid? user = null) => new CommandContext { HttpQueued = false, Name = name, User = user, CommandId = CommandId.CreateNew(), CorrelationId = CorrelationId.CreateNew(), WSQueued = false };
    }
}
