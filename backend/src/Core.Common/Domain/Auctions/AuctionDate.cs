using System;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class AuctionDate
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


        public override bool Equals(object obj) => obj is AuctionDate && ((AuctionDate)obj).Value.Equals(this.Value);
        public override int GetHashCode() => this.Value.GetHashCode();
        public override string ToString() => this.Value.ToString();

        public static implicit operator AuctionDate(DateTime dateTime) => new AuctionDate(dateTime);
        public static implicit operator DateTime(AuctionDate dateTime) => dateTime.Value;
    }
}