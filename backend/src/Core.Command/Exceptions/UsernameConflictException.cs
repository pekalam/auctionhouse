using System;

namespace Core.Command.Exceptions
{
    public class UsernameConflictException : CommandException
    {
        public UsernameConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UsernameConflictException(string message) : base(message)
        {
        }
    }
}