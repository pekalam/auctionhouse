using Auctions.Domain;
using Core.DomainFramework;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class Tag_Tests
    {
        [Theory]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void Constructor_when_invalid_tag_value_throws(string value)
        {
            Assert.Throws<DomainException>(() => new Tag(value));
        }

        [Theory]
        [InlineData("example", "example")]
        [InlineData(" example", "example")]
        [InlineData("example ", "example")]
        [InlineData(" example ", "example")]
        [InlineData("             example                ", "example")]
        public void Value_gets_trimmed(string value, string excpected)
        {
            var tag = new Tag(value);
            tag.Value.Should()
                .Be(excpected);
        }

        [Fact]
        public void Constructor_when_tag_value_is_too_long_throws()
        {
            var tag = Enumerable.Range(0, TagConstants.MAX_LENGTH + 1)
                .Select(i => $"{i}")
                .Aggregate((s, s1) => s + s1);
            Assert.Throws<DomainException>(() => new Tag(tag));
        }
    }
}
