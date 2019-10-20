using System;

namespace Web
{
    public class ApiExcpetion : Exception
    {
        public ApiExcpetion(string message) : base(message)
        {
        }

        public ApiExcpetion(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}