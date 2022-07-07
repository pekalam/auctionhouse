namespace Common.Application.Commands
{
    public class InvalidCommandException : Exception
    {
        public CommandContext? CommandContext { get; }

        public InvalidCommandException(string message, CommandContext? commandContext = null) : base(message)
        {
            CommandContext = commandContext;
        }

        public InvalidCommandException(string message, Exception innerException, CommandContext? commandContext = null) : base(message, innerException)
        {
            CommandContext = commandContext;
        }
    }
}