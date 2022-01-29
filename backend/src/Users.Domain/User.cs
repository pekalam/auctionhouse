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
        public UserPaymentsId? UserPaymentsId { get; private set; }

        public static User Create(Username username, decimal initialCredits = 0)
        {
            var user = new User()
            {
                AggregateId = UserId.New(),
                Username = username,
                Credits = initialCredits,
            };
            user.AddEvent(new UserCreated(user.AggregateId, username, initialCredits));
            return user;
        }

        public User()
        {
        }

        private void ApplyEvent(UserCreated @event)
        {
            AggregateId = @event.UserId;
            Username = new Username(@event.Username);
            Credits = @event.InitialCredits;
        }

        public void AssignUserPayments(UserPaymentsId userPaymentsId)
        {
            if(UserPaymentsId is not null)
            {
                return;
            }

            UserPaymentsId = userPaymentsId;
            AddEvent(new UserRegistered(AggregateId, Username, Credits));
        }
        private void ApplyEvent(UserRegistered @event)
        {
        }


        public void AddCredits(decimal toAdd)
        {
            ApplyEvent(AddEvent(new CreditsAdded(toAdd, AggregateId)));
        }
        private void ApplyEvent(CreditsAdded ev)
        {
            Credits += ev.CreditsCount;
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

            ApplyEvent(AddEvent(new LockedFundsCreated { 
                LockedFundsId = lockedFundsId.Value,
                Amount = amount,
            }));
        }
        private void ApplyEvent(LockedFundsCreated ev)
        {
            Credits -= ev.Amount;
            _lockedFunds.Add(new LockedFunds(new LockedFundsId(ev.LockedFundsId), ev.Amount));
        }

        public void UnlockCredits(LockedFundsId lockedFundsId)
        {
            var lockedFunds = _lockedFunds.FirstOrDefault(f => f.Id.Value == lockedFundsId.Value);
            if (lockedFunds is null)
            {
                throw new DomainException("Could not find LockedFunds with id " + lockedFundsId.Value);
            }

            ApplyEvent(AddEvent(new CreditsLocked { LockedFundsId = lockedFundsId.Value, Amount = lockedFunds.Amount, UserId = AggregateId }));
        }
        private void ApplyEvent(CreditsLocked ev)
        {
            Credits += ev.Amount;
            _lockedFunds.Remove(_lockedFunds.First(l => l.Id.Value == ev.LockedFundsId));
        }

        public void WithdrawCredits(LockedFundsId lockedFundsId)
        {
            var lockedFunds = _lockedFunds.FirstOrDefault(f => f.Id.Value == lockedFundsId.Value);
            if (lockedFunds is null)
            {
                throw new DomainException("Could not find LockedFunds with id " + lockedFundsId.Value);
            }

            
            ApplyEvent(AddEvent(new CreditsWithdrawn(lockedFunds.Amount, AggregateId,lockedFundsId.Value)));
        }
        private void ApplyEvent(CreditsWithdrawn ev)
        {
            _lockedFunds.Remove(_lockedFunds.First(l => l.Id.Value == ev.LockedFundsId));
        }
    }
}
