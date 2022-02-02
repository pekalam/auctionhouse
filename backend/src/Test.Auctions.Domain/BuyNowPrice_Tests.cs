using Auctions.Domain;
using Core.DomainFramework;
using Xunit;

namespace Test.AuctionsDomain
{
    public class BuyNowPrice_Tests
    {
        [Fact]
        public void Cannot_be_lower_or_equal_min_value()
        {
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MIN_VALUE));
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MIN_VALUE - 1));
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MIN_VALUE + 0.006m));
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MIN_VALUE + 0.004m));
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MIN_VALUE + 0.005m));
        }

        [Fact]
        public void Cannot_be_greater_than_max_value()
        {
            _ = new BuyNowPrice(BuyNowPrice.MAX_VALUE - 1m);
            _ = new BuyNowPrice(BuyNowPrice.MAX_VALUE);
            _ = new BuyNowPrice(BuyNowPrice.MAX_VALUE + 0.006m);
            _ = new BuyNowPrice(BuyNowPrice.MAX_VALUE + 0.004m);
            _ = new BuyNowPrice(BuyNowPrice.MAX_VALUE + 0.005m);
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MAX_VALUE + 0.1m));
            Assert.Throws<DomainException>(() => new BuyNowPrice(BuyNowPrice.MAX_VALUE + 0.01m));
        }
    }
}
