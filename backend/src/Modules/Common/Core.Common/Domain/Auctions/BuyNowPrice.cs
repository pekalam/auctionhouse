using System.Collections.Generic;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
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

        public override string ToString() => this.Value.ToString();

        public static implicit operator BuyNowPrice(decimal value) => new BuyNowPrice(value);
        public static implicit operator decimal(BuyNowPrice buyNowPrice) => buyNowPrice.Value;
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}