using System;

namespace Core.Command.Exceptions
{
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message)
        {
        }

        public CommandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
