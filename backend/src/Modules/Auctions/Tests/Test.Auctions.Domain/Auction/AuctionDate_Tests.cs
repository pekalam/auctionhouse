using Auctions.Domain;
using Core.DomainFramework;
using FluentAssertions;
using System;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class AuctionDate_Tests
    {
        [Fact]
        public void Constructor_fails_when_not_in_utc()
        {
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.Now)).Message.Should().Be("Auction date is not in UTC format");
        }

        [Fact]
        public void Constructor_fails_when_max_value()
        {
            var date = DateTime.MaxValue;
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            Assert.Throws<DomainException>(() => new AuctionDate(date)).Message.Should().Be("Auction date cannot be max or min datetime value");
        }

        [Fact]
        public void Constructor_fails_when_min_value()
        {
            var date = DateTime.MinValue;
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            Assert.Throws<DomainException>(() => new AuctionDate(date)).Message.Should().Be("Auction date cannot be max or min datetime value");
        }
    }
}
