using System;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Users.UsernameProfanity
{
    public class UsernameProfanityFoundException : DomainException
    {
        public string Username { get; }

        public UsernameProfanityFoundException(string username, string message) : base(message)
        {
            Username = username;
        }

        public UsernameProfanityFoundException(string username, string message, Exception innerException) : base(message, innerException)
        {
            Username = username;
        }
    }
}