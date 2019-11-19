using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class BuyNowPrice
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

        public override bool Equals(object obj) => obj is BuyNowPrice && ((BuyNowPrice)obj).Value.Equals(this.Value);
        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => this.Value.ToString();

        public static implicit operator BuyNowPrice(decimal value) => new BuyNowPrice(value);
        public static implicit operator decimal(BuyNowPrice buyNowPrice) => buyNowPrice.Value;
    }
}