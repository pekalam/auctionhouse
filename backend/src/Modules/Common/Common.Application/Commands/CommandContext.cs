using Common.Application.Events;

namespace Common.Application.Commands
{
    public class CommandContext
    {
        //todo: not mutable extra data
        private static readonly Dictionary<string, string> _nullExtraData = new Dictionary<string, string>();

        public CommandId CommandId { get; init; }
        public CorrelationId CorrelationId { get; init; }
        public Guid? User { get; set; }
        public bool HttpQueued { get; set; }
        public bool WSQueued { get; set; }
        public string Name { get; init; }
        public Dictionary<string, string> ExtraData { get; set; }

        public CommandContext(CommandId commandId, CorrelationId correlationId, Guid? user, bool httpQueued, bool wsQueued, string name, Dictionary<string, string>? extraData)
        {
            CommandId = commandId;
            CorrelationId = correlationId;
            User = user;
            HttpQueued = httpQueued;
            WSQueued = wsQueued;
            Name = name;
            ExtraData = extraData ?? _nullExtraData;
        }

        public static CommandContext CreateNew(string name, Guid? user = null) => new(CommandId.CreateNew(), CorrelationId.CreateNew(), user, false, false, name, new Dictionary<string, string>());

        public CommandContext CloneWithName(string name) => new(new CommandId(CommandId.Id), CorrelationId.Value, User, HttpQueued, WSQueued, name, new Dictionary<string, string>(ExtraData));
    }
}
