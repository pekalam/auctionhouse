using System;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.DomainModelTests
{
    public class AuctionTestUtils
    {
        private static int UserNum = 0;

        public static Auction CreateBuyNowOnlyAuction()
        {
            var args = new AuctionArgs.Builder()
                .SetBuyNowOnly(true)
                .SetOwner(new UserIdentity())
                .SetCategory(new Category("", 1))
                .SetBuyNow(123)
                .SetStartDate(DateTime.UtcNow.AddDays(1))
                .SetEndDate(DateTime.UtcNow.AddDays(2))
                .SetProduct(new Product("test name", "desccription 1111", Condition.New))
                .SetTags(new[] { "tag1" })
                .SetName("Test name")
                .Build();
            return new Auction(args);
        }

        public static User CreateUser(decimal credits = 1000)
        {
            var user = new User();
            user.Register($"test username {UserNum++}");
            user.AddCredits(credits);
            user.MarkPendingEventsAsHandled();
            return user;
        }
    }
}