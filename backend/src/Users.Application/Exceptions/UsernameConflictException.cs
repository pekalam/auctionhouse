namespace Users.Domain.Exceptions
{
    public class UsernameConflictException : Exception
    {
        public UsernameConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UsernameConflictException(string message) : base(message)
        {
        }
    }
}