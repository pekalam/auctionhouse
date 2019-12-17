using System;
using Core.Common.Command;

namespace Core.Command.Exceptions
{
    public class InvalidCommandException : CommandException
    {
        public CommandContext CommandContext { get; }

        public InvalidCommandException(string message, CommandContext commandContext = null) : base(message)
        {
            CommandContext = commandContext;
        }

        public InvalidCommandException(string message, CommandContext commandContext, Exception innerException) : base(message, innerException)
        {
            CommandContext = commandContext;
        }
    }
}