using Auctions.Tests.Base.Domain.ModelBuilders;
using Core.DomainFramework;
using FluentAssertions;
using System;
using Xunit;

namespace Auctions.Domain.Tests
{
    [Trait("Category", "Unit")]
    public class Auction_Construction_Tests
    {

        [Fact]
        public void Created_auction_should_have_not_null_bids_id_after_assigning_bids()
        {
            var auction = new GivenAuction().WithAuctionArgs(new GivenAuctionArgs().ValidForBuyNowAndBidAuctionType()).Build();
            var expectedAuctionBids = new AuctionBidsId(Guid.NewGuid());
            auction.AuctionBidsId.Should().BeNull();

            auction.AssignAuctionBids(expectedAuctionBids);

            auction.AuctionBidsId.Should().Be(expectedAuctionBids);
        }

        [Fact]
        public void Cannot_create_buynowonly_action_with_null_price()
        {
            var auctionArgs = new GivenAuctionArgs()
                .WithBuyNowOnly(true).Build();
            auctionArgs.BuyNowPrice = null;

            Assert.Throws<DomainException>(() => new GivenAuction().WithAuctionArgs(auctionArgs).Build());
        }
    }
}
