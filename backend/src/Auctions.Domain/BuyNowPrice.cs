using Core.Common.Domain;
using Core.DomainFramework;

namespace Auctions.Domain
{
    public class BuyNowPrice : ValueObject
    {
        public const decimal MIN_VALUE = 0.00m;
        public const decimal MAX_VALUE = 100_000_000m;

        public decimal Value { get; }

        public BuyNowPrice(decimal value) //TODO money type for multi currency support
        {
            if (decimal.Round(value, 2, MidpointRounding.ToZero) <= MIN_VALUE)
            {
                throw new DomainException("Too low buy now price value");
            }
            if(decimal.Round(value, 2, MidpointRounding.ToZero) > MAX_VALUE)
            {
                throw new DomainException("Price is too high");
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