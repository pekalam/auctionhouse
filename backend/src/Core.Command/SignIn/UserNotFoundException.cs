using System;
using Core.Common.Exceptions.Command;

namespace Core.Command.SignIn
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