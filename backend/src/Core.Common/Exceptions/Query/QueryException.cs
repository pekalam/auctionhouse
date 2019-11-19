using System;

namespace Core.Common.Exceptions.Query
{
    public class QueryException : Exception
    {
        public QueryException(string message) : base(message)
        {
        }

        public QueryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
