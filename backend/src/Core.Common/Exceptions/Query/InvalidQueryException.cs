using System;

namespace Core.Common.Exceptions.Query
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