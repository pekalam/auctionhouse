using Auctions.Domain;
using Core.DomainFramework;
using System;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class AuctionDate_Tests
    {
        [Fact]
        public void Constructor_when_invalid_value_throws()
        {
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.MaxValue));
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.MinValue));
            Assert.Throws<DomainException>(() => new AuctionDate(DateTime.Now));
        }
    }
}
