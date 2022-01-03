using Auctions.Domain;
using System;
using static Test.Auctions.Domain.AuctionTestConstants;

namespace Test.Auctions.Domain
{
    public class GivenAuction
    {
        public Auction ValidOfTypeBuyNowAndBid()
        {
            var auctionArgsBuilder = new AuctionArgs.Builder()
                .SetBuyNow(BUY_NOW_PRICE)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(5))
                .SetOwner(UserId.New())
                .SetProduct(new Product(PRODUCT_NAME, PRODUCT_DESCRIPTION, Condition.New))
                .SetBuyNowOnly(false)
                .SetTags(new[] { TAG_1 })
                .SetName(NAME);
            auctionArgsBuilder.SetCategories(CATEGORY_IDS, new TestConvertCategoryNamesToRootToLeafIds()).GetAwaiter().GetResult();
            return new Auction(auctionArgsBuilder.Build());
        }
    }
}
