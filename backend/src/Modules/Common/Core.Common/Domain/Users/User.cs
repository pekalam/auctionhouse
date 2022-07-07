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

    public partial class User : AggregateRoot<User, UserId, UserUpdateEventGroup>
    {
        public const int MIN_USERNAME_LENGTH = 4;


        public decimal Credits { get; private set; }
        public Username Username { get; private set; }

        public static User Create(Username username)
        {
            var user = new User()
            {
                AggregateId = UserId.New(),
                Username = username
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
