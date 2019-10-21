using System;
using Core.Command.Exceptions;

namespace Core.Command.SignIn
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