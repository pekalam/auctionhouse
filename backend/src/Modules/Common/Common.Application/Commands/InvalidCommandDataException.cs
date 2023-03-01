namespace Common.Application.Commands
{
    public class InvalidCommandDataException : Exception
    {
        public CommandContext? CommandContext { get; }

        public InvalidCommandDataException(string message, CommandContext? commandContext = null) : base(message)
        {
            CommandContext = commandContext;
        }

        public InvalidCommandDataException(string message, Exception innerException, CommandContext? commandContext = null) : base(message, innerException)
        {
            CommandContext = commandContext;
        }
    }
}