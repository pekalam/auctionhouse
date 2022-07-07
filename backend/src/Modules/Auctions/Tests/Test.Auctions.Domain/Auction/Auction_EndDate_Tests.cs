using Auctions.Tests.Base.Domain.ModelBuilders;
using Core.DomainFramework;
using FluentAssertions;
using Xunit;

namespace Auctions.Domain.Tests
{
    public class Auction_EndDate_Tests
    {
        private Auction auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

        [Fact]
        public void Updating_end_date_fails_if_set_to_start_date()
        {
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate)).Message.Should().Be("Auction does not last long enough");
        }

        [Fact]
        public void Updating_end_date_fails_if_set_to_date_earlier_than_start_date()
        {
            Assert.Throws<DomainException>(() => auction.UpdateEndDate(auction.StartDate.Value.AddDays(-1))).Message.Should().Be("Auction does not last long enough");
        }

        [Fact]
        public void Updating_end_date_succeeds_if_end_date_is_past_current_end_date()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            var end = auction.EndDate;

            auction.UpdateEndDate(end.Value.AddDays(12));

            auction.EndDate.Value.Should().Be(end.Value.AddDays(12));
        }
    }
}
