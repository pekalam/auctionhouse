using Auctions.Domain;
using Core.DomainFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auctions.Tests.Base.Builders;
using Xunit;

namespace Auctions.Domain.Tests
{

    [Trait("Category", "Unit")]
    public class AuctionCreation_Tests
    {

        [Fact]
        public void Created_buy_now_only_auction_should_be_unlocked()
        {
            var auctionArgs = new GivenAuctionArgs().WithBuyNowOnly(true).Build();
            var auction = new GivenAuction().WithAuctionArgs(auctionArgs).Build();

            auction.Locked.Should().BeFalse();
        }

        [Fact]
        public void Created_auction_should_be_locked_and_unlocked_when_bids_id_is_set()
        {
            var auction = new GivenAuction().WithAuctionArgs(new GivenAuctionArgs().ValidBuyNowAndBid()).Build();

            auction.Locked.Should().BeTrue();

            auction.AddAuctionBids(new AuctionBidsId(Guid.NewGuid()));
        }

        [Fact]
        public void Cannot_create_buynowonly_action_with_null_price()
        {
            Assert.Throws<DomainException>(() => new GivenAuctionArgs()
                .WithBuyNowOnly(true)
                .WithBuyNowOnlyPrice(null).Build());

            var auctionArgs = new GivenAuctionArgs()
                .WithBuyNowOnly(true).Build();
            auctionArgs.BuyNowPrice = null;
            Assert.Throws<DomainException>(() => new GivenAuction().WithAuctionArgs(auctionArgs).Build());

        }
    }
}
