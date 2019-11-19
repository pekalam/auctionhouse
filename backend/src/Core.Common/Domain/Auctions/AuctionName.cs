using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class AuctionName
    {
        public const int MAX_LENGTH = 40;

        public string Value { get; }

        public AuctionName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Invalid auction name value");
            }
            if (value.Trim().Length > MAX_LENGTH)
            {
                throw new DomainException("Auction name is too long");
            }
            Value = value.Trim();
        }

        public override bool Equals(object obj) => obj is AuctionName && ((AuctionName)obj).Value.Equals(this.Value);
        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => this.Value.ToString();

        public static implicit operator AuctionName(string value) => new AuctionName(value);
        public static implicit operator string(AuctionName auctionName) => auctionName.Value;
    }
}