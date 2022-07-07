using Core.Common.Domain;

namespace Users.Domain
{
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
}