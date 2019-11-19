using System;
using System.Collections.Generic;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users.Events;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Users
{
    public class InvalidUsernameException : DomainException
    {
        public InvalidUsernameException(string message) : base(message)
        {
        }
    }

    public partial class User : AggregateRoot<User>
    {
        public const int MIN_USERNAME_LENGTH = 4;


        public UserIdentity UserIdentity { get; private set; }

        public User()
        {
            
        }

        private object CheckIsRegistered() => UserIdentity ?? throw new DomainException($"User not registered");

        public void Register(string username)
        {
            if (UserIdentity != null)
            {
                throw new DomainException($"User {username} is already registered");
            }
            if (username.Length < MIN_USERNAME_LENGTH)
            {
                throw new InvalidUsernameException("Too short username");
            }
            UserIdentity = new UserIdentity()
            {
                UserId = AggregateId,
                UserName = username
            };
            AddEvent(new UserRegistered(UserIdentity));
        }

    }
}
