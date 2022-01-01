using Auctions.Domain.Services;
using Core.DomainFramework;

namespace Auctions.Domain
{
    public class AuctionArgs
    {
        public BuyNowPrice BuyNowPrice { get; set; } = null!;
        public bool BuyNowOnly { get; set; }
        public AuctionImage?[] AuctionImages { get; set; } = new AuctionImage[0]; //passing is not required
        public AuctionDate StartDate { get; set; } = null!;
        public AuctionDate EndDate { get; set; } = null!;
        public UserId Owner { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public CategoryId[] Categories { get; set; } = null!;
        public Tag[] Tags { get; set; } = null!;
        public AuctionName Name { get; set; } = null!;

        public class Builder
        {
            private AuctionArgs args = new AuctionArgs();

            private void CheckCanBuild()
            {
                //TODO null object pattern?
                if (args.Product == null ||
                    args.StartDate == null ||
                    args.EndDate == null ||
                    args.Owner == null ||
                    args.Tags == null ||
                    args.Name == null ||
                    args.AuctionImages == null ||
                    args.Categories == null || args.Categories.Length != 3
                )
                {
                    throw new DomainException("Invalid auctionArgs");
                }
            }

            public Builder From(AuctionArgs args)
            {
                this.args = args;
                return this;
            }

            public Builder SetTags(string[] tags)
            {
                args.Tags = tags.Select(s => (Tag)s).ToArray();
                return this;
            }
            public Builder SetTags(Tag[] tags)
            {
                args.Tags = tags;
                return this;
            }
            public Builder SetBuyNow(BuyNowPrice buyNowPrice)
            {
                args.BuyNowPrice = buyNowPrice;
                return this;
            }

            public Builder SetBuyNowOnly(bool buyNowOnly)
            {
                args.BuyNowOnly = buyNowOnly;
                return this;
            }

            public Builder SetStartDate(AuctionDate startDate)
            {
                args.StartDate = startDate;
                return this;
            }

            public Builder SetEndDate(AuctionDate endDate)
            {
                args.EndDate = endDate;
                return this;
            }

            public Builder SetOwner(UserId owner)
            {
                args.Owner = owner;
                return this;
            }

            public Builder SetProduct(Product product)
            {
                args.Product = product;
                return this;
            }

            public async Task<Builder> SetCategories(string[] categoryNames, IConvertCategoryNamesToRootToLeafIds convertCategoryNamesToIds)
            {
                args.Categories = await convertCategoryNamesToIds.ConvertNames(categoryNames);
                return this;
            }

            public Builder SetImages(AuctionImage?[] images)
            {
                args.AuctionImages = images;
                return this;
            }

            public Builder SetName(AuctionName name)
            {
                args.Name = name;
                return this;
            }

            public AuctionArgs Build()
            {
                CheckCanBuild();
                return args;
            }
        }
    }
}