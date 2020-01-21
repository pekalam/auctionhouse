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

    public class UserUpdateEventGroup : UpdateEventGroup
    {
        public UserUpdateEventGroup() : base("userUpdated")
        {
        }
    }

    public partial class User : AggregateRoot<User, UserUpdateEventGroup>
    {
        public const int MIN_USERNAME_LENGTH = 4;


        public UserIdentity UserIdentity { get; private set; }
        public decimal Credits { get; private set; } = 0;

        public User()
        {
        }

        private object ThrowIfNotRegistered() => UserIdentity ?? throw new DomainException("User is not registered");

        public void CheckIsRegistered() => ThrowIfNotRegistered();

        public bool IsRegistered => UserIdentity != null;

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

        public void AddCredits(decimal toAdd)
        {
            ThrowIfNotRegistered();
            Credits += toAdd;
            AddEvent(new CreditsAdded(toAdd, UserIdentity));
        }

        public void WithdrawCredits(decimal toWithdraw)
        {
            ThrowIfNotRegistered();
            if (Credits < toWithdraw)
            {
                throw new DomainException("Not enough credits");
            }

            Credits -= toWithdraw;

            AddEvent(new CreditsWithdrawn(toWithdraw, UserIdentity));
        }

        public void ReturnCredits(decimal toReturn)
        {
            ThrowIfNotRegistered();
            if (toReturn == 0)
            {
                throw new DomainException("Cannot return 0 credits");
            }
            Credits += toReturn;

            AddEvent(new CreditsReturned(toReturn, UserIdentity));
        }

        public void CancelCredits(decimal ammount)
        {
            ThrowIfNotRegistered();
            Credits -= ammount;

            AddEvent(new CreditsCanceled(ammount, UserIdentity));
        }
    }
}
