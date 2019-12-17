using System;

namespace Core.Command.Exceptions
{
    public class InvalidPasswordException : CommandException
    {
        public InvalidPasswordException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidPasswordException(string message) : base(message)
        {
        }
    }
}