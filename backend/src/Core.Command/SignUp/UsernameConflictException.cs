using System;
using Core.Common.Exceptions.Command;

namespace Core.Command.SignUp
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