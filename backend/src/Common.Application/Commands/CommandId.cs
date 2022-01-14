namespace Common.Application.Commands
{
    /// <summary>
    /// It can be used to wait for command to complete it's work. Used instead of correlation id.
    /// </summary>
    public class CommandId
    {
        public string Id { get; private set; }

        public CommandId(string id)
        {
            Id = id;
        }

        public static CommandId CreateNew() => new CommandId(Guid.NewGuid().ToString());

        public static CommandId From(string s) => new CommandId(s);
    }
}
