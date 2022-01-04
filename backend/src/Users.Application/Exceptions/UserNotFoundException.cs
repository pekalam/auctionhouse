namespace Users.Domain.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UserNotFoundException(string message) : base(message)
        {
        }
    }
}