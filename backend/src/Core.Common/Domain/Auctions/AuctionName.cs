using System.Collections.Generic;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class AuctionName : ValueObject
    {
        public const int MAX_LENGTH = 40;
        public const int MIN_LENGTH = 5;

        public string Value { get; }

        private AuctionName(){}

        public AuctionName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Invalid auction name value");
            }

            if (value.Trim().Length < MIN_LENGTH)
            {
                throw new DomainException("Auction name is too short");
            }
            if (value.Trim().Length > MAX_LENGTH)
            {
                throw new DomainException("Auction name is too long");
            }
            Value = value.Trim();
        }

        public override string ToString() => this.Value.ToString();

        public static implicit operator AuctionName(string value) => new AuctionName(value);
        public static implicit operator string(AuctionName auctionName) => auctionName.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}