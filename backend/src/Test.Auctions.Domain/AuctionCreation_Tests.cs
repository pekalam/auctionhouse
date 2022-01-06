using Auctions.Domain;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Auctions.Base.Builders;
using Xunit;

namespace Test.AuctionsDomain
{

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

    }
}
