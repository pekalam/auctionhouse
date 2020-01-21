using System;

namespace Core.Query.Exceptions
{
    public class ResourceNotFoundException : QueryException
    {
        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}