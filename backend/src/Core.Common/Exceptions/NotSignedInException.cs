using System;

namespace Core.Common.Exceptions
{
    public class NotSignedInException : Exception
    {
        public NotSignedInException(string message) : base(message)
        {
        }

        public NotSignedInException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}