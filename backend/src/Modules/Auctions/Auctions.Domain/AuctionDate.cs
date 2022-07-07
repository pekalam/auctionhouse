using Core.Common.Domain;
using Core.DomainFramework;

namespace Auctions.Domain
{
    public class AuctionDate : ValueObject
    {
        public DateTime Value { get; }

        public AuctionDate(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new DomainException("Auction date is not in UTC format");
            }
            if (value.Equals(DateTime.MinValue) || value.Equals(DateTime.MaxValue))
            {
                throw new DomainException("Auction date cannot be max or min datetime value");
            }

            Value = value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator AuctionDate(DateTime dateTime) => new AuctionDate(dateTime);
        public static implicit operator DateTime(AuctionDate dateTime) => dateTime.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}