using System;

namespace Core.Query.Exceptions
{
    public class InvalidQueryException : QueryException
    {
        public InvalidQueryException(string message) : base(message)
        {
        }

        public InvalidQueryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}