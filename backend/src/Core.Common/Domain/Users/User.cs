using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
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

    public class UserUpdateEventGroup : UpdateEventGroup<UserId>
    {
        public UserUpdateEventGroup() : base("userUpdated")
        {
        }
    }

    public class Username : ValueObject
    {
        public string Value { get; private set; }

        internal Username(string value)
        {
            Value = value;
        }

        public static async Task<Username> Create(string username, IUsernameProfanityCheck profanityCheck = null) //TODO
        {
            if (profanityCheck == null) profanityCheck = new NullUsernameProfanityCheck();


            if (username.Length < User.MIN_USERNAME_LENGTH)
            {
                throw new InvalidUsernameException("Too short username");
            }
            if (await profanityCheck.CheckIsSatisfyingConditions(username))
            {
                return new Username(username);
            }

            throw new UsernameProfanityFoundException(username, $"Found username profanity: {username}");
        }

        public override string ToString() => Value;
        public static implicit operator string(Username username) => username.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

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

    public interface IUsernameProfanityCheck //TODO
    {
        Task<bool> CheckIsSatisfyingConditions(string username);
    }

    class NullUsernameProfanityCheck : IUsernameProfanityCheck
    {
        public Task<bool> CheckIsSatisfyingConditions(string username)
        {
            return Task.FromResult(true);
        }
    }

    public class UserId : ValueObject
    {
        public static readonly UserId Empty = new UserId(Guid.Empty);

        public Guid Value { get; }

        public UserId(Guid value)
        {
            Value = value;
        }

        public static UserId New() => new UserId(Guid.NewGuid());

        public override string ToString() => Value.ToString();
        public static implicit operator Guid(UserId id) => id.Value;
        public static implicit operator UserId(Guid guid) => new UserId(guid);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    public partial class User : AggregateRoot<User, UserId, UserUpdateEventGroup>
    {
        public const int MIN_USERNAME_LENGTH = 4;


        public decimal Credits { get; private set; }
        public Username Username { get; private set; }

        public static User Create(Username username)
        {
            var user = new User()
            {
                AggregateId = UserId.New(), Username = username
            };
            user.AddEvent(new UserRegistered(user.AggregateId, username.Value));
            return user;
        }

        public User()
        {
        }



        public void CheckIsRegistered()
        {
        }

        public bool IsRegistered => true;

        public void AddCredits(decimal toAdd)
        {
            Credits += toAdd;
            AddEvent(new CreditsAdded(toAdd, AggregateId));
        }

        public void WithdrawCredits(decimal toWithdraw)
        {
            if (Credits < toWithdraw)
            {
                throw new DomainException("Not enough credits");
            }

            Credits -= toWithdraw;

            AddEvent(new CreditsWithdrawn(toWithdraw, AggregateId));
        }

        public void ReturnCredits(decimal toReturn)
        {
            if (toReturn == 0)
            {
                throw new DomainException("Cannot return 0 credits");
            }
            Credits += toReturn;

            AddEvent(new CreditsReturned(toReturn, AggregateId));
        }

        public void CancelCredits(decimal ammount)
        {
            Credits -= ammount;

            AddEvent(new CreditsCanceled(ammount, AggregateId));
        }
    }
}
