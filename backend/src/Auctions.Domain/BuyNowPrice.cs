using Core.Common.Domain;
using Core.DomainFramework;

namespace Auctions.Domain
{
    public class BuyNowPrice : ValueObject
    {
        public const int MIN_VALUE = 0;

        public decimal Value { get; }

        public BuyNowPrice(decimal value)
        {
            if (value < MIN_VALUE)
            {
                throw new DomainException("Too low buy now price value");
            }
            Value = value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator BuyNowPrice(decimal value) => new BuyNowPrice(value);
        public static implicit operator decimal(BuyNowPrice buyNowPrice) => buyNowPrice.Value;
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}