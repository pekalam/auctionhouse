using System;

namespace Core.Command.Exceptions.Common
{
    public class UserNotSignedInException : CommandException
    {
        public UserNotSignedInException(string message) : base(message)
        {
        }

        public UserNotSignedInException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
