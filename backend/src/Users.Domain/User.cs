using Core.DomainFramework;
using Users.Domain;
using Users.Domain.Events;
using Users.Domain.Shared;
using Users.DomainEvents;

namespace Core.Common.Domain.Users
{
    using static UserConstants;

    public class InvalidUsernameException : DomainException
    {
        public InvalidUsernameException(string message) : base(message)
        {
        }
    }

    public class UserConstants
    {
        public const int MIN_USERNAME_LENGTH = 4;
    }

    public class LockedFundsId : GuidId<LockedFundsId>
    {
        public LockedFundsId(Guid value) : base(value)
        {
        }

        public static LockedFundsId New() => new LockedFundsId(Guid.NewGuid());
    }

    public class LockedFunds : SimpleEntity<LockedFundsId>
    {
        public decimal Amount { get; }

        public LockedFunds(LockedFundsId lockedFundsId, decimal amount)
        {
            Id = lockedFundsId;
            Amount = amount;
        }
    }

    public partial class User : AggregateRoot<User, UserId, UserUpdateEventGroup>
    {
        private List<LockedFunds> _lockedFunds = new();
        public decimal Credits { get; private set; }
        public Username Username { get; private set; }
        public IReadOnlyList<LockedFunds> LockedFunds => _lockedFunds;

        public static User Create(Username username, decimal initialCredits = 0)
        {
            var user = new User()
            {
                AggregateId = UserId.New(),
                Username = username,
                Credits = initialCredits,
            };
            user.AddEvent(new UserRegistered(user.AggregateId, username.Value, initialCredits));
            return user;
        }

        public User()
        {
        }


        public void AddCredits(decimal toAdd)
        {
            ApplyEvent(AddEvent(new CreditsAdded(toAdd, AggregateId)));
        }

        public void LockCredits(LockedFundsId lockedFundsId, decimal amount)
        {
            if (Credits < amount)
            {
                throw new DomainException("Not enough credits");
            }
            if(_lockedFunds.Select(f => f.Id).Contains(lockedFundsId))
            {
                throw new DomainException("There is already registered lockedFunds entity with id " + lockedFundsId.Value);
            }

            var lockedFunds = new LockedFunds(lockedFundsId, amount);
            _lockedFunds.Add(lockedFunds);
            AddEvent(new LockedFundsCreated { 
                LockedFundsId = lockedFunds.Id,
                Amount = amount,
            });
        }

        public void WithdrawCredits(LockedFundsId lockedFundsId)
        {
            var lockedFunds = _lockedFunds.FirstOrDefault(f => f.Id.Value == lockedFundsId.Value);
            if (lockedFunds is null)
            {
                throw new DomainException("Could not find LockedFunds with id " + lockedFundsId.Value);
            }

            Credits -= lockedFunds.Amount;
            _lockedFunds.Remove(lockedFunds);
            
            AddEvent(new CreditsWithdrawn(lockedFunds.Amount, AggregateId,lockedFundsId.Value));
        }
    }
}
