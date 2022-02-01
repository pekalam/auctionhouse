using Core.Common.Domain;

namespace Auctions.Domain
{
    public class CategoryId : ValueObject
    {
        public int Value { get; }

        public CategoryId(int value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();
        public static implicit operator CategoryId(int value) => new CategoryId(value);
        public static implicit operator int(CategoryId id) => id.Value;
    }
}