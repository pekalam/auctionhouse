using System.Collections.Generic;
using System.Linq;
using Core.Common.Exceptions;

namespace Core.Common.Domain.Auctions
{
    public class Tag : ValueObject
    {
        public const int MAX_LENGTH = 30;
        public const int MIN_LENGTH = 1;

        public string Value { get;  }

        public Tag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Invalid tag value");
            }
            if (value.Trim().Length > MAX_LENGTH)
            {
                throw new DomainException("Tag is too long");
            }
            Value = value.Trim();
        }

        public static Tag[] From(string[] tags) => tags.Select(s => new Tag(s)).ToArray();

        public override string ToString() => this.Value;

        public static implicit operator Tag(string value) => new Tag(value);
        public static implicit operator string(Tag obj) => obj.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}