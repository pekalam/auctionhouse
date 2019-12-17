using System;

namespace Core.Command.Exceptions
{
    public class UserNotFoundException : CommandException
    {
        public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}