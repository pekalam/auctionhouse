using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using FluentAssertions;
using NUnit.Framework;

namespace Core.DomainModelTests
{
    [TestFixture]
    public class Tag_Tests
    {
        [TestCase(" ")]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_when_invalid_tag_value_throws(string value)
        {
            Assert.Throws<DomainException>(() => new Tag(value));
        }

        [TestCase("example", "example")]
        [TestCase(" example", "example")]
        [TestCase("example ", "example")]
        [TestCase(" example ", "example")]
        [TestCase("             example                ", "example")]
        public void Value_gets_trimmed(string value, string excpected)
        {
            var tag = new Tag(value);
            tag.Value.Should()
                .Be(excpected);
        }

        [Test]
        public void Constructor_when_tag_value_is_too_long_throws()
        {
            string tag = Enumerable.Range(0, Tag.MAX_LENGTH + 1)
                .Select(i => $"{i}")
                .Aggregate((s, s1) => s + s1);
            Assert.Throws<DomainException>(() => new Tag(tag));
        }
    }

    [TestFixture]
    public class AuctionDate_Tests
    {
        [Test]
        public void Constructor_when_invalid_value_throws()
        {
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.MaxValue));
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.MinValue));
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.Now));
        }
    }

    [TestFixture]
    public class AuctionName_Tests
    {
        [TestCase(" ")]
        [TestCase("")]
        [TestCase(null)]
        public void Constructor_when_invalid_value_throws(string value)
        {
            Assert.Throws<DomainException>(() => new AuctionName(value));
        }

        [TestCase("example", "example")]
        [TestCase(" example", "example")]
        [TestCase("example ", "example")]
        [TestCase(" example ", "example")]
        [TestCase("             example                ", "example")]
        public void Value_gets_trimmed(string value, string excpected)
        {
            var name = new AuctionName(value);
            name.Value.Should()
                .Be(excpected);
        }

        [Test]
        public void Constructor_when_tag_value_is_too_long_throws()
        {
            string name = Enumerable.Range(0, AuctionName.MAX_LENGTH + 1)
                .Select(i => $"{i}")
                .Aggregate((s, s1) => s + s1);
            Assert.Throws<DomainException>(() => new AuctionName(name));
        }
    }
}
