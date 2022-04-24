using Auctions.Tests.Base.Domain.ModelBuilders;
using Auctions.Tests.Base.Domain.ModelBuilders.Shared;
using Core.DomainFramework;
using FluentAssertions;
using Xunit;

namespace Auctions.Domain.Tests
{
    [Trait("Category", "Unit")]
    public class AuctionCreateSession_Tests
    {
        [Fact]
        public void Throws_when_adding_or_replacing_and_max_time_of_session_reached()
        {
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            var img = new GivenAuctionImage().Build();
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.AddOrReplaceImage(img, 0));
        }

        [Fact]
        public void CreateAuction_when_session_max_time_reached_throws()
        {
            var auctionArgs = new GivenAuctionArgs().ValidForBuyNowAndBidAuctionType();
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.CreateAuction(auctionArgs));
        }

        [Fact]
        public void ResetSession_when_session_max_time_not_reached_resets()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            var img = new GivenAuctionImage().Build();
            session.AddOrReplaceImage(img, 0);
            session.ResetSession();
            session.AuctionImages[0]
                .Should()
                .BeNull();
        }

        [Fact]
        public void ResetSession_when_session_max_time_throws()
        {
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.ResetSession());
        }

        [Fact]
        public void AddOrReplaceImage_adds_to_auction_images()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            var imgNum = 1;

            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            var image1 = new GivenAuctionImage().Build();
            session.AddOrReplaceImage(image1, imgNum);

            session.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
            for (var i = 0; i < session.AuctionImages.Count; i++)
            {
                if (i == imgNum)
                {
                    continue;
                }

                session.AuctionImages[i]
                    .Should()
                    .BeNull();
            }

            session.AuctionImages[imgNum]
                .Should()
                .Be(image1);
            session.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
        }

        [Fact]
        public void AddOrReplaceImage_replaces_image()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            var imgNum = 1;

            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            var image1 = new GivenAuctionImage().Build();
            var image2 = new GivenAuctionImage().Build();
            session.AddOrReplaceImage(image1, imgNum);
            session.AddOrReplaceImage(image2, imgNum);

            session.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
            for (var i = 0; i < session.AuctionImages.Count; i++)
            {
                if (i == imgNum)
                {
                    continue;
                }

                session.AuctionImages[i]
                    .Should()
                    .BeNull();
            }

            session.AuctionImages[imgNum]
                .Should()
                .Be(image2);
            session.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
        }

        [Fact]
        public void CreateAuction_when_not_null_image_adds_it_to_auction_images()
        {
            var auctionArgs = new GivenAuctionArgs().ValidForBuyNowAndBidAuctionType();
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            var image1 = new GivenAuctionImage().Build();
            var image2 = new GivenAuctionImage().Build();
            session.AddOrReplaceImage(image1, 0);
            session.AddOrReplaceImage(image2, 1);

            var auction = session.CreateAuction(auctionArgs);

            auction.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
            auction.AuctionImages[0]
                .Should()
                .Be(image1);
            auction.AuctionImages[1]
                .Should()
                .Be(image2);
            for (var i = 2; i < auction.AuctionImages.Count; i++)
            {
                auction.AuctionImages[i]
                    .Should()
                    .BeNull();
            }
        }

        [Fact]
        public void CreateAuction_null_image_in_session_creates_auction_without_images()
        {
            var auctionArgs = new GivenAuctionArgs().ValidForBuyNowAndBidAuctionType();
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            var session = AuctionCreateSession.CreateSession(GivenUserId.Build());
            session.AddOrReplaceImage(null, 0);

            var auction = session.CreateAuction(auctionArgs);

            auction.AuctionImages.Count.Should()
                .Be(Auction.MAX_IMAGES);
            for (var i = 0; i < auction.AuctionImages.Count; i++)
            {
                auction.AuctionImages[i]
                    .Should()
                    .BeNull();
            }
        }
    }
}
