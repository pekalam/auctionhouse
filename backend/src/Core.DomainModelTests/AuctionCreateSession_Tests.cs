using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using Core.Common.Domain;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Core.DomainModelTests
{
    [TestFixture]
    public class AuctionCreateSession_Tests
    {
        private User user = null;
        private string username = "test_username";
        private AuctionArgs auctionArgs;

        [SetUp]
        public void SetUp()
        {
            user = new User();
            auctionArgs = new AuctionArgs.Builder()
                .SetBuyNow(12)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(1))
                .SetProduct(new Product("name", "desc", Condition.New))
                .SetCategory(new Category("", 0))
                .SetOwner(new UserIdentity())
                .SetTags(new []{"tag1"})
                .SetName("Test name")
                .Build();
        }

        [Test]
        public void AddOrReplaceImage_when_session_max_time_reached_throws()
        {
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            var img = new AuctionImage("1", "2", "3");
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.AddOrReplaceImage(img, 0));
        }

        [Test]
        public void CreateAuction_when_session_max_time_reached_throws()
        {
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.CreateAuction(auctionArgs));
        }

        [Test]
        public void ResetSession_when_session_max_time_not_reached_resets()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            var img = new AuctionImage("1", "2", "3");
            session.AddOrReplaceImage(img, 0);
            session.ResetSession();
            session.AuctionImages[0].Should().BeNull();
        }

        [Test]
        public void ResetSession_when_session_max_time_throws()
        {
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            AuctionCreateSession.SESSION_MAX_TIME = -1;
            Assert.Throws<DomainException>(() => session.ResetSession());
        }

        [Test]
        public void AddOrReplaceImage_not_null_image_adds_to_auction_images()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            int imgNum = 1;

            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            var image1 = new AuctionImage("id1", "id2", "id3");
            session.AddOrReplaceImage(image1, imgNum);

            session.AuctionImages.Count.Should().Be(Auction.MAX_IMAGES);
            for (int i = 0; i < session.AuctionImages.Count; i++)
            {
                if (i == imgNum)
                {
                    continue;
                }

                session.AuctionImages[i].Should().BeNull();
            }

            session.AuctionImages[imgNum].Should().Be(image1);
            session.AuctionImages.Count.Should().Be(Auction.MAX_IMAGES);
        }

        [Test]
        public void CreateAuction_when_not_null_image_adds_it_to_auction_images()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            var image1 = new AuctionImage("id1", "id2", "id3");
            session.AddOrReplaceImage(image1, 0);
            var image2 = new AuctionImage("id1", "id2", "id3");
            session.AddOrReplaceImage(image2, 1);

            var auction = session.CreateAuction(auctionArgs);

            auction.AuctionImages.Count.Should().Be(Auction.MAX_IMAGES);
            auction.AuctionImages[0].Should().Be(image1);
            auction.AuctionImages[1].Should().Be(image2);
            for (int i = 2; i < auction.AuctionImages.Count; i++)
            {
                auction.AuctionImages[i].Should().BeNull();
            }
        }

        [Test]
        public void CreateAuction_null_image_in_session_creates_auction_without_images()
        {
            AuctionCreateSession.SESSION_MAX_TIME = AuctionCreateSession.DEFAULT_SESSION_MAX_TIME;
            user.Register(username);
            var session = user.UserIdentity.GetAuctionCreateSession();
            session.AddOrReplaceImage(null, 0);

            var auction = session.CreateAuction(auctionArgs);

            auction.AuctionImages.Count.Should().Be(Auction.MAX_IMAGES);
            for (int i = 0; i < auction.AuctionImages.Count; i++)
            {
                auction.AuctionImages[i].Should().BeNull();
            }
        }
    }
}