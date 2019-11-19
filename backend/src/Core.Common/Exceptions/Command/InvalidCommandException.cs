using System;

namespace Core.Common.Exceptions.Command
{
    public class InvalidCommandException : CommandException
    {
        public InvalidCommandException(string message) : base(message)
        {
        }

        public InvalidCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}