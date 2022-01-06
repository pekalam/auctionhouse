using Auctions.Domain;
using Test.Auctions.Base.Mocks;
using static Test.Auctions.Base.Builders.AuctionTestConstants;

namespace Test.Auctions.Base.Builders
{
    public class GivenAuctionArgs
    {
        private BuyNowPrice _buyNowPrice = BUY_NOW_PRICE;
        private UserId _ownerId = UserId.New();
        private Product _product = new Product(PRODUCT_NAME, PRODUCT_DESCRIPTION, Condition.New);
        private string[] _tags = new[] { TAG_1 };
        private AuctionName _name = NAME;
        private string[] _categories = CATEGORY_IDS;
        private bool _buyNowOnly = false;

        public GivenAuctionArgs WithBuyNowOnly(bool buyNowOnly)
        {
            _buyNowOnly = buyNowOnly;
            return this;
        }

        public AuctionArgs Build()
        {
            var auctionArgsBuilder = new AuctionArgs.Builder()
                .SetBuyNow(_buyNowPrice)
                .SetStartDate(DateTime.UtcNow.AddMinutes(20))
                .SetEndDate(DateTime.UtcNow.AddDays(5))
                .SetOwner(_ownerId)
                .SetProduct(_product)
                .SetBuyNowOnly(_buyNowOnly)
                .SetTags(_tags)
                .SetName(_name);
            auctionArgsBuilder.SetCategories(_categories, new TestConvertCategoryNamesToRootToLeafIds()).GetAwaiter().GetResult();
            return auctionArgsBuilder.Build();
        }

        public AuctionArgs ValidBuyNowAndBid()
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
            return auctionArgsBuilder.Build();
        }
    }
}
