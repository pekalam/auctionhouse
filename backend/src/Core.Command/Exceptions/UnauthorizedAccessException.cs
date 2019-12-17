using System;

namespace Core.Command.Exceptions
{
    public class UnauthorizedAccessException : CommandException
    {
        public UnauthorizedAccessException(string message) : base(message)
        {
        }

        public UnauthorizedAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}